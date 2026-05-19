const token = localStorage.getItem('jwt_token');

if (!token) window.location.href = '/index.html';

// 1. JWT TOKEN PARSING
function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) { return null; }
}

const userData = parseJwt(token);
const userRoles = userData["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || userData.role;

if (userRoles === "Admin" || (Array.isArray(userRoles) && userRoles.includes("Admin"))) {
    const adminBtn = document.getElementById('admin-link');
    if (adminBtn) {
        adminBtn.classList.remove('hidden');
        adminBtn.onclick = () => {
            window.location.href = '/admin.html';
        };
    }
}

// 2. LOAD CATEGORIES
async function loadCategories() {
    try {
        const res = await fetch('/api/catalog/categories');
        if (!res.ok) throw new Error("Failed to load categories");

        const categories = await res.json();
        const container = document.getElementById('categories-container');

        const firstBtn = container.firstElementChild;
        container.innerHTML = '';
        if (firstBtn) container.appendChild(firstBtn);

        categories.forEach(cat => {
            const btn = document.createElement('button');
            btn.className = "bg-white border border-gray-200 text-gray-700 px-5 py-2 rounded-full whitespace-nowrap hover:border-indigo-500 hover:text-indigo-600 transition";
            btn.innerText = cat.name;
            btn.onclick = () => loadProducts(cat.id);
            container.appendChild(btn);
        });
    } catch (err) {
        console.error(err);
    }
}

// 3. LOAD PRODUCTS
async function loadProducts(categoryId = null) {
    const url = categoryId ? `/api/catalog/products/category/${categoryId}` : '/api/catalog/products';
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error("Failed to load products");

        const products = await res.json();
        const grid = document.getElementById('products-grid');
        grid.innerHTML = '';

        products.forEach(p => {
            grid.innerHTML += `
                <div class="bg-white p-5 rounded-2xl border border-gray-100 shadow-sm hover:shadow-md transition">
                    <div class="h-40 bg-gray-100 rounded-xl mb-4 flex items-center justify-center text-gray-400 font-bold tracking-widest uppercase text-xs">
                        ${p.name} Image
                    </div>
                    <h3 class="font-bold text-lg mb-1">${p.name}</h3>
                    <p class="text-gray-500 text-sm mb-4 line-clamp-2">${p.description}</p>
                    <div class="flex justify-between items-center">
                        <span class="text-xl font-black text-indigo-600">$${p.price}</span>
                        <div class="flex items-center gap-2">
                            <input type="number" id="qty-${p.id}" value="1" min="1" max="${p.stockQuantity}" class="w-12 p-1 border rounded text-center font-bold">
                            <button onclick="addToCart('${p.id}', '${p.name}', ${p.price})" class="bg-black text-white px-4 py-2 rounded-lg hover:bg-gray-800 transition font-bold text-sm">
                                Add
                            </button>
                        </div>
                    </div>
                    <p class="text-[10px] mt-2 text-gray-400 font-bold uppercase tracking-tighter">Stock: ${p.stockQuantity} units</p>
                </div>
            `;
        });
    } catch (err) {
        console.error(err);
    }
}

// --- BASKET LOGIC ---

async function addToCart(productId, productName, price) {
    const quantityInput = document.getElementById(`qty-${productId}`);
    const quantity = parseInt(quantityInput.value);

    let basket = await fetchBasket();

    const existingItem = basket.items.find(i => i.productId === productId);
    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        basket.items.push({
            productId: productId,
            productName: productName,
            price: price,
            quantity: quantity
        });
    }

    await updateBasket(basket);
    renderBasket(basket);
}

async function fetchBasket() {
    try {
        const res = await fetch('/api/ordering/basket', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (res.ok) return await res.json();
    } catch (err) {
        console.error("Basket fetch error:", err);
    }
    return { items: [] };
}

async function updateBasket(basket) {
    try {
        await fetch('/api/ordering/basket', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(basket)
        });
    } catch (err) {
        console.error("Update basket error:", err);
    }
}

function renderBasket(basket) {
    const container = document.getElementById('cart-items');
    const totalEl = document.getElementById('cart-total');
    const countEl = document.getElementById('cart-count');

    container.innerHTML = '';
    let total = 0;
    let count = 0;

    if (!basket || !basket.items || basket.items.length === 0) {
        container.innerHTML = '<p class="text-gray-400 text-center py-4 text-sm font-medium uppercase tracking-widest">Basket is empty</p>';
    } else {
        basket.items.forEach(item => {
            const itemTotal = item.price * item.quantity;
            total += itemTotal;
            count += item.quantity;

            container.innerHTML += `
                <div class="flex justify-between items-center border-b border-gray-50 pb-3">
                    <div>
                        <p class="font-bold text-sm text-gray-800">${item.productName}</p>
                        <p class="text-xs text-gray-400 font-bold uppercase tracking-tighter">${item.quantity} x $${item.price}</p>
                    </div>
                    <span class="font-bold text-indigo-600 text-sm">$${itemTotal.toFixed(2)}</span>
                </div>
            `;
        });
    }

    totalEl.innerText = `$${total.toFixed(2)}`;
    countEl.innerText = count;

    const checkoutBtn = document.getElementById('checkout-btn');
    if (checkoutBtn) {
        checkoutBtn.disabled = !basket || !basket.items || basket.items.length === 0;
    }
}

// --- ORDER LOGIC ---

document.getElementById('checkout-btn').onclick = async () => {
    const basket = await fetchBasket();

    if (!basket.items || basket.items.length === 0) return;

    const orderData = {
        items: basket.items.map(i => ({
            productId: i.productId,
            quantity: i.quantity
        }))
    };

    try {
        const response = await fetch('/api/ordering/orders', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(orderData)
        });

        if (response.ok) {
            alert("SUCCESS: Order has been placed!");
            await fetch('/api/ordering/basket', {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });
            renderBasket({ items: [] });
            loadProducts();
        } else {
            const error = await response.json();
            alert("ORDER FAILED: " + (error.message || "Unknown error"));
        }
    } catch (err) {
        alert("CRITICAL ERROR: Connection failed");
    }
};

// 9. INITIALIZATION
document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    loadProducts();
    fetchBasket().then(renderBasket);
});

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    loadProducts();
    fetchBasket().then(renderBasket);
});

function logout() {
    console.log("Logging out...");
    localStorage.removeItem('jwt_token');
    window.location.href = '/index.html';
}

const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn) {
    logoutBtn.onclick = logout;
}