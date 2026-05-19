const token = localStorage.getItem('jwt_token');

if (!token) window.location.href = '/index.html';

const productNameCache = {};
let allOrders = [];
let currentFilter = 'all';

document.addEventListener('DOMContentLoaded', loadOrders);

async function loadOrders() {
    try {
        const res = await fetch('/api/ordering/orders', {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Failed to fetch orders");

        allOrders = await res.json();
        renderFilteredOrders();
    } catch (err) {
        console.error(err);
        document.getElementById('orders-container').innerHTML = `
            <div class="bg-red-50 text-red-600 p-4 rounded-2xl text-center font-bold">
                Error loading orders. Please try again later.
            </div>`;
    }
}

function setFilter(filter) {
    currentFilter = filter;

    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.classList.remove('active', 'text-white');
        btn.classList.add('text-gray-600');
    });

    const activeBtn = document.getElementById(`btn-${filter}`);
    activeBtn.classList.add('active');
    activeBtn.classList.remove('text-gray-600');

    renderFilteredOrders();
}

async function renderFilteredOrders() {
    const container = document.getElementById('orders-container');

    const filtered = allOrders.filter(order => {
        const status = (order.status || "").toLowerCase();
        if (currentFilter === 'active') return status !== 'cancelled';
        if (currentFilter === 'cancelled') return status === 'cancelled';
        return true;
    });

    if (filtered.length === 0) {
        container.innerHTML = `
            <div class="text-center py-20 bg-white rounded-3xl border border-dashed border-gray-200 text-gray-400 font-medium">
                No orders found in this category.
            </div>`;
        return;
    }

    container.innerHTML = '<div class="text-center text-gray-400 animate-pulse">Syncing product names...</div>';

    let html = '';

    for (const order of filtered) {
        let itemsHtml = '';
        for (const item of order.items) {
            const name = await fetchProductName(item.productId);
            const price = item.unitPrice || 0;
            const subtotal = price * item.quantity;

            itemsHtml += `
                <div class="flex justify-between items-center py-4 border-b border-gray-50 last:border-0">
                    <div class="flex flex-col">
                        <span class="font-bold text-gray-800 text-lg">${name}</span>
                        <span class="text-xs text-gray-400 font-medium">${item.quantity} UNIT(S) x $${price.toFixed(2)}</span>
                    </div>
                    <span class="font-bold text-gray-900">$${subtotal.toFixed(2)}</span>
                </div>`;
        }

        html += `
        <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden mb-6 transition hover:shadow-md">
            <div class="px-6 py-4 bg-gray-50/50 border-b flex justify-between items-center">
                <div class="flex flex-col">
                    <span class="text-[10px] font-black text-gray-400 uppercase tracking-widest">Order ID</span>
                    <span class="text-xs font-mono text-gray-500">${order.id.substring(0, 12)}...</span>
                </div>
                <div class="text-right">
                    <span class="text-[10px] font-black text-gray-400 uppercase tracking-widest">Status</span>
                    <div class="text-sm font-black ${getStatusClass(order.status)}">${order.status}</div>
                </div>
            </div>
            
            <div class="p-6">
                <div class="mb-6">${itemsHtml}</div>
                <div class="flex justify-between items-end border-t border-gray-100 pt-6">
                    <div>
                        <p class="text-[10px] font-black uppercase text-gray-400 mb-1 tracking-widest">Grand Total</p>
                        <p class="text-3xl font-black text-indigo-600">$${(order.totalPrice || 0).toFixed(2)}</p>
                    </div>
                    ${(order.status !== 'Cancelled' && order.status !== 'Completed') ?
                `<button onclick="cancelOrder('${order.id}')" 
                                class="bg-red-50 text-red-600 px-6 py-3 rounded-2xl text-sm font-bold hover:bg-red-600 hover:text-white transition shadow-sm">
                            Cancel Order
                        </button>`
                : ''}
                </div>
            </div>
        </div>`;
    }

    container.innerHTML = html;
}

async function fetchProductName(productId) {
    if (productNameCache[productId]) return productNameCache[productId];
    try {
        const res = await fetch(`/api/catalog/products/${productId}`);
        if (!res.ok) return "Product " + productId.substring(0, 4);
        const data = await res.json();
        productNameCache[productId] = data.name;
        return data.name;
    } catch {
        return "Unknown Product";
    }
}

function getStatusClass(status) {
    const s = (status || "").toLowerCase();
    if (s === 'pending' || s === 'created') return 'text-amber-500';
    if (s === 'completed') return 'text-emerald-500';
    if (s === 'cancelled') return 'text-red-400';
    return 'text-indigo-500';
}

async function cancelOrder(orderId) {
    if (!confirm("Are you sure you want to cancel this order?")) return;

    try {
        const res = await fetch(`/api/ordering/orders/${orderId}/cancel`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        if (res.ok) {
            await loadOrders();
        } else {
            alert("Failed to cancel order.");
        }
    } catch (err) {
        console.error("Cancel error:", err);
    }
}