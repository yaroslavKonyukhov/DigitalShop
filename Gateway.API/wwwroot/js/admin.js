const token = localStorage.getItem('jwt_token');
if (!token) window.location.href = '/index.html';

let isEditing = false;

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    loadAdminProducts();
    loadUsers();
});

function logout() {
    localStorage.removeItem('jwt_token');
    window.location.href = '/index.html';
}

// --- CATEGORY OPERATIONS ---

async function loadCategories() {
    const res = await fetch('/api/catalog/categories');
    const categories = await res.json();

    const select = document.getElementById('p-category');
    select.innerHTML = '<option value="">Select Category</option>';

    const catList = document.getElementById('admin-category-list');
    catList.innerHTML = '';

    categories.forEach(c => {
        const opt = document.createElement('option');
        opt.value = c.id;
        opt.innerText = c.name;
        select.appendChild(opt);

        catList.innerHTML += `
            <div class="flex justify-between items-center p-3 bg-gray-50 rounded-lg group">
                <span class="font-medium text-gray-700">${c.name}</span>
                <button onclick="deleteCategory('${c.id}')" class="text-xs text-red-400 hover:text-red-600 opacity-0 group-hover:opacity-100 transition">Delete</button>
            </div>
        `;
    });
}

document.getElementById('category-form').onsubmit = async (e) => {
    e.preventDefault();
    const name = document.getElementById('cat-name').value;
    const res = await fetch('/api/catalog/categories', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ name })
    });
    if (res.ok) {
        document.getElementById('cat-name').value = '';
        loadCategories();
    }
};

async function deleteCategory(id) {
    if (!confirm("Deleting a category might affect products. Continue?")) return;
    const res = await fetch(`/api/catalog/categories/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (res.ok) loadCategories();
}

// --- PRODUCT OPERATIONS ---

async function loadAdminProducts() {
    const res = await fetch('/api/catalog/products?includeOutOfStock=true');
    const products = await res.json();
    const tbody = document.getElementById('admin-product-list');
    tbody.innerHTML = '';

    products.forEach(p => {
        tbody.innerHTML += `
            <tr class="border-b border-gray-50 hover:bg-gray-50 transition">
                <td class="p-4">
                    <div class="font-bold text-gray-800">${p.name}</div>
                    <div class="text-xs text-gray-400">ID: ${p.id.substring(0, 8)}...</div>
                </td>
                <td class="p-4 font-semibold text-indigo-600">$${p.price}</td>
                <td class="p-4">
                    <span class="${p.stockQuantity === 0 ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'} px-2 py-1 rounded text-xs font-bold">
                        ${p.stockQuantity} items
                    </span>
                </td>
                <td class="p-4 text-right space-x-2">
                    <button onclick='editProductMode(${JSON.stringify(p).replace(/'/g, "&apos;")})' class="text-xs bg-indigo-50 text-indigo-600 px-3 py-1 rounded hover:bg-indigo-600 hover:text-white transition font-bold">Edit</button>
                    <button onclick="deleteProduct('${p.id}')" class="text-xs bg-red-50 text-red-600 px-3 py-1 rounded hover:bg-red-600 hover:text-white transition font-bold">Delete</button>
                </td>
            </tr>
        `;
    });
}

function editProductMode(product) {
    isEditing = true;
    document.getElementById('form-title').innerText = "Edit Product";
    document.getElementById('submit-btn').innerText = "Update Product";
    document.getElementById('submit-btn').classList.replace('bg-indigo-600', 'bg-amber-500');
    document.getElementById('cancel-edit').classList.remove('hidden');

    document.getElementById('edit-product-id').value = product.id;
    document.getElementById('p-name').value = product.name;
    document.getElementById('p-desc').value = product.description;
    document.getElementById('p-price').value = product.price;
    document.getElementById('p-stock').value = product.stockQuantity;
    document.getElementById('p-category').value = product.categoryId;
}

document.getElementById('cancel-edit').onclick = () => resetForm();

function resetForm() {
    isEditing = false;
    document.getElementById('product-form').reset();
    document.getElementById('form-title').innerText = "Create New Product";
    document.getElementById('submit-btn').innerText = "Add Product";
    document.getElementById('submit-btn').classList.contains('bg-amber-500') && document.getElementById('submit-btn').classList.replace('bg-amber-500', 'bg-indigo-600');
    document.getElementById('cancel-edit').classList.add('hidden');
    document.getElementById('edit-product-id').value = '';
}

document.getElementById('product-form').onsubmit = async (e) => {
    e.preventDefault();
    const id = document.getElementById('edit-product-id').value;
    const productData = {
        name: document.getElementById('p-name').value,
        description: document.getElementById('p-desc').value,
        price: parseFloat(document.getElementById('p-price').value),
        stockQuantity: parseInt(document.getElementById('p-stock').value),
        categoryId: document.getElementById('p-category').value
    };

    const url = isEditing ? `/api/catalog/products/${id}` : '/api/catalog/products';
    const method = isEditing ? 'PUT' : 'POST';

    const res = await fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(productData)
    });

    if (res.ok) {
        alert(isEditing ? "Product updated!" : "Product added!");
        resetForm();
        loadAdminProducts();
    } else {
        alert("Error: Check your permissions or fields");
    }
};

async function deleteProduct(id) {
    if (!confirm("Are you sure?")) return;
    const res = await fetch(`/api/catalog/products/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (res.ok) loadAdminProducts();
}

// --- USER MANAGEMENT (IDENTITY) ---

async function loadUsers() {
    const tbody = document.getElementById('users-table-body');
    try {
        const res = await fetch('/api/identity/auth/users', {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error();
        const users = await res.json();

        tbody.innerHTML = users.map(user => `
            <tr class="hover:bg-gray-50 transition">
                <td class="p-4 font-medium text-gray-700">${user.email}</td>
                <td class="p-4">
                    <span class="px-2 py-1 rounded text-[10px] font-black uppercase ${user.isAdmin ? 'bg-amber-100 text-amber-600' : 'bg-blue-50 text-blue-500'}">
                        ${user.isAdmin ? 'Admin' : 'User'}
                    </span>
                </td>
                <td class="p-4 text-right">
                    ${!user.isAdmin ? `
                        <button onclick="promoteToAdmin('${user.id}')" class="text-[10px] font-black uppercase text-indigo-600 hover:underline">Make Admin</button>
                    ` : '<span class="text-[10px] text-gray-300 font-bold uppercase italic">System Admin</span>'}
                </td>
            </tr>
        `).join('');
    } catch {
        tbody.innerHTML = '<tr><td colspan="3" class="p-4 text-center text-red-400">Failed to load users</td></tr>';
    }
}

async function promoteToAdmin(userId) {
    if (!confirm("Promote this user to Admin?")) return;
    const res = await fetch(`/api/identity/auth/assign-admin/${userId}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` }
    });
    if (res.ok) {
        alert("User promoted!");
        loadUsers();
    }
}