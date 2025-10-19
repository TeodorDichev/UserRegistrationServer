const form = document.getElementById('loginForm');

form.addEventListener('submit', async e => {
    e.preventDefault();
    document.getElementById('formError').textContent = '';

    const data = {
        email: form.email.value,
        password: form.password.value
    };

    const res = await fetch('/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });

    const result = await res.json();

    if (res.status === 200 && result.Success) {
        window.location.href = '/home.html';
    } else {
        document.getElementById('formError').textContent = result.Message || 'Login failed';
    }
});
