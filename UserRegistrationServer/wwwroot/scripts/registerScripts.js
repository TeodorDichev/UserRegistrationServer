const form = document.getElementById('registerForm');

form.addEventListener('submit', async e => {
    e.preventDefault();
    document.querySelectorAll('.error').forEach(span => span.textContent = '');
    const data = {
        firstName: form.firstName.value,
        middleName: form.middleName.value,
        lastName: form.lastName.value,
        email: form.email.value,
        password: form.password.value,
        confirmPassword: form.confirmPassword.value,
        captcha: form.captcha.value
    };

    const res = await fetch('/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });

    const result = await res.json();

    if (res.status === 200 && result.Success) {
        localStorage.setItem('user', JSON.stringify(result.User));
        window.location.href = '/home.html';
    } else if (result.Errors) {
        for (const field in result.Errors) {
            const span = document.getElementById(`${field}Error`);
            if (span) span.textContent = result.Errors[field];
        }
        if (result.Message) {
            document.getElementById('formError').textContent = result.Message;
        }
    } else {
        document.getElementById('formError').textContent = result.Message || "Error";
    }
});
