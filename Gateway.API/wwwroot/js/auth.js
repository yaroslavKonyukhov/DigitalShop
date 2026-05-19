const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const toggleBtn = document.getElementById('toggle-form');
const formTitle = document.getElementById('form-title');
const messageDiv = document.getElementById('message');

toggleBtn.addEventListener('click', () => {
    const isLoginActive = !loginForm.classList.contains('hidden');
    loginForm.classList.toggle('hidden');
    registerForm.classList.toggle('hidden');

    if (isLoginActive) {
        formTitle.innerText = "Sign Up";
        formTitle.classList.replace('text-indigo-600', 'text-emerald-600');
        toggleBtn.innerText = "Already have an account? Sign In";
    } else {
        formTitle.innerText = "Sign In";
        formTitle.classList.replace('text-emerald-600', 'text-indigo-600');
        toggleBtn.innerText = "Don't have an account? Sign Up";
    }
    messageDiv.classList.add('hidden');
});

// LOGIN ACTION
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
        email: document.getElementById('login-email').value,
        password: document.getElementById('login-password').value
    };

    try {
        const response = await fetch('/api/identity/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (response.ok) {
            localStorage.setItem('jwt_token', result.token);
            showStatus("Login success! Redirecting...", "bg-green-100 text-green-800");

            setTimeout(() => {
                window.location.href = '/shop.html';
            }, 1000);
        } else {
            showStatus("Login failed: Invalid credentials", "bg-red-100 text-red-800");
        }
    } catch (err) {
        showStatus("System Error: Identity service is unreachable", "bg-red-100 text-red-800");
    }
});

// REGISTER ACTION
registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const data = {
        firstName: document.getElementById('reg-firstname').value,
        lastName: document.getElementById('reg-lastname').value,
        email: document.getElementById('reg-email').value,
        password: document.getElementById('reg-password').value
    };

    try {
        const response = await fetch('/api/identity/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showStatus("Account created successfully! Switching to Login...", "bg-green-100 text-green-800");
            setTimeout(() => toggleBtn.click(), 2000);
        } else {
            const error = await response.text();
            showStatus("Registration Error: " + error, "bg-red-100 text-red-800");
        }
    } catch (err) {
        showStatus("Network Error: Could not connect to gateway", "bg-red-100 text-red-800");
    }
});

function showStatus(text, classes) {
    messageDiv.innerText = text;
    messageDiv.className = `mt-6 p-4 rounded-lg text-sm text-center ${classes}`;
    messageDiv.classList.remove('hidden');
}