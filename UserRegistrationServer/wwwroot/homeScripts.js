document.addEventListener('DOMContentLoaded', () => {
    // ===== Load user info =====
    const raw = localStorage.getItem('user');
    let user = {};

    try {
        if (raw) user = JSON.parse(raw);
    } catch {
        // Corrupted or "undefined" entry
        localStorage.removeItem('user');
        window.location.href = '/index.html';
        return;
    }

    if (!user.Email) {
        window.location.href = '/login.html';
        return;
    }
    document.getElementById('firstName').textContent = user.FirstName || '';
    document.getElementById('firstNameH').textContent = user.FirstName || '';
    document.getElementById('middleName').textContent = user.MiddleName || '';
    document.getElementById('lastName').textContent = user.LastName || '';
    document.getElementById('lastNameH').textContent = user.LastName || '';
    document.getElementById('email').textContent = user.Email;

    // ===== Logout =====
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('user');
            window.location.href = '/login.html';
        });
    }

    // ===== Update Names =====
    const updateForm = document.getElementById('updateForm');
    updateForm.addEventListener('submit', async e => {
        e.preventDefault();

        const data = {};
        if (updateForm.firstName.value.trim()) data.firstName = updateForm.firstName.value.trim();
        if (updateForm.middleName.value.trim()) data.middleName = updateForm.middleName.value.trim();
        if (updateForm.lastName.value.trim()) data.lastName = updateForm.lastName.value.trim();
        data.email = user.Email;

        try {
            const res = await fetch('/update-names', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await res.json();

            if (res.status === 200) {
                // Update localStorage and UI for updated fields only
                const user = JSON.parse(localStorage.getItem('user'));
                if (data.firstName) {
                    user.FirstName = data.firstName;
                    document.getElementById('firstName').textContent = data.firstName;
                }
                if (data.middleName) {
                    user.MiddleName = data.middleName;
                    document.getElementById('middleName').textContent = data.middleName;
                }
                if (data.lastName) {
                    user.lastName = data.lastName;
                    document.getElementById('lastName').textContent = data.lastName;
                }
                localStorage.setItem('user', JSON.stringify(user));
                alert('Name(s) updated successfully');
                updateForm.reset();
            } else {
                alert(result.message || 'Failed to update names');
            }
        } catch (err) {
            console.error(err);
            alert('Server error while updating names');
        }
    });

    // ===== Update Password =====
    const passwordForm = document.getElementById('passwordForm');
    passwordForm.addEventListener('submit', async e => {
        e.preventDefault();
        const user = JSON.parse(localStorage.getItem('user') || '{}');

        const data = {
            currentPassword: passwordForm.currentPassword.value,
            newPassword: passwordForm.newPassword.value,
            confirmPassword: passwordForm.confirmPassword.value,
            email: user.Email
        };

        try {
            const res = await fetch('/update-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await res.json();

            if (res.status === 200) {
                alert('Password updated successfully');
                passwordForm.reset();
            } else {
                alert(result.message || 'Failed to update password');
            }
        } catch (err) {
            console.error(err);
            alert('Server error while updating password');
        }
    });
});
