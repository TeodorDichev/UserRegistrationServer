document.addEventListener('DOMContentLoaded', () => {
    async function loadUser() {
        try {
            const res = await fetch('/me', {
                method: 'GET',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' }
            });
            if (!res.ok) {
                window.location.href = '/index.html';
                return;
            }
            const user = await res.json();
            document.getElementById('firstName').textContent = user.FirstName || '';
            document.getElementById('firstNameH').textContent = user.FirstName || '';
            document.getElementById('middleName').textContent = user.MiddleName || '';
            document.getElementById('lastName').textContent = user.LastName || '';
            document.getElementById('lastNameH').textContent = user.LastName || '';
            document.getElementById('email').textContent = user.Email;
        } catch {
            window.location.href = '/index.html';
        }
    }
    loadUser();

    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async () => {
            await fetch('/logout', { method: 'POST', credentials: 'include' });
            window.location.href = '/index.html';
        });
    }

    const updateForm = document.getElementById('updateForm');
    updateForm.addEventListener('submit', async e => {
        e.preventDefault();

        const data = {};
        if (updateForm.firstName.value.trim()) data.firstName = updateForm.firstName.value.trim();
        if (updateForm.middleName.value.trim()) data.middleName = updateForm.middleName.value.trim();
        if (updateForm.lastName.value.trim()) data.lastName = updateForm.lastName.value.trim();

        try {
            const res = await fetch('/update-names', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await res.json();
            if (res.ok) {
                alert('Name(s) updated successfully');
                updateForm.reset();
                await loadUser();
            } else {
                alert(result.Message || 'Failed to update names');
            }
        } catch (err) {
            console.error(err);
            alert('Server error while updating names');
        }
    });

    const passwordForm = document.getElementById('passwordForm');
    passwordForm.addEventListener('submit', async e => {
        e.preventDefault();
        const data = {
            currentPassword: passwordForm.currentPassword.value,
            newPassword: passwordForm.newPassword.value,
            confirmPassword: passwordForm.confirmPassword.value
        };

        try {
            const res = await fetch('/update-password', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await res.json();
            if (res.ok) {
                alert('Password updated successfully');
                passwordForm.reset();
            } else {
                alert(result.Message || 'Failed to update password');
            }
        } catch (err) {
            console.error(err);
            alert('Server error while updating password');
        }
    });
});
