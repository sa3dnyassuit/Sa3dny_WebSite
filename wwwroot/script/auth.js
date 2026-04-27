const API_BASE_URL = 'https://recite-slander-riverboat.ngrok-free.dev/api/Auth';

const registerForm = document.getElementById('registerForm');
if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        
        const fullName = document.getElementById('regFullName').value.trim();
        const phone = document.getElementById('regPhone').value.trim();
        const location = document.getElementById('regLocation').value;
        const email = document.getElementById('regEmail').value.trim();
        const password = document.getElementById('regPassword').value;
        const confirmPassword = document.getElementById('regConfirmPassword').value;

        
        if (!fullName || !phone || !location || !email || !password || !confirmPassword) {
            alert("برجاء إكمال كافة الحقول المطلوبة.");
            return;
        }

        
        if (password !== confirmPassword) {
            alert("كلمتا المرور غير متطابقتين!");
            return;
        }

        const customerData = {
            Name: fullName,
            Phone: phone,
            Email: email,
            Password: password,
            ConfirmPassword: confirmPassword,
            LocationName: location
        };

        try {
            const response = await fetch(`${API_BASE_URL}/register/customer`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(customerData)
            });

            const result = await response.json();

            if (response.ok) {
                alert("تم إنشاء الحساب بنجاح! يمكنك الآن تسجيل الدخول.");
                window.location.href = 'login.html';
            } else {
                // التعامل مع الأخطاء الراجعة من الـ API (مثل الإيميل موجود مسبقاً)
                if (response.status === 400 && result.message === "Email already exists") {
                    alert("هذا البريد الإلكتروني مسجل بالفعل، جرب بريداً آخر.");
                } else if (result.errors) {
                    // عرض أخطاء الـ Identity (مثل الباسورد ضعيف)
                    let errorMsg = Object.values(result.errors).flat().join('\n');
                    alert(errorMsg);
                } else {
                    alert(result.message || "فشل التسجيل، تأكد من البيانات المدخلة.");
                }
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("خطأ في الاتصال بالسيرفر، تأكد من تشغيل الـ API.");
        }
    });
}

// ======================== ثانياً: كود تسجيل الدخول (Login) ========================
const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const email = document.getElementById('loginEmail').value.trim();
        const password = document.getElementById('loginPassword').value;

        
        if (!email || !password) {
            alert("برجاء إدخال البريد الإلكتروني وكلمة المرور.");
            return;
        }

        const loginData = {
            Email: email,
            Password: password
        };

        try {
            const response = await fetch(`${API_BASE_URL}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(loginData)
            });

            const result = await response.json();

            if (response.ok) {
                
                localStorage.setItem('token', result.token);
                localStorage.setItem('userName', result.name);
                localStorage.setItem('userRole', result.role);

                alert(`أهلاً بك مجدداً يا ${result.name}!`);
                window.location.href = '../pages/index.html';
            } else {
                
                if (response.status === 401) {
                    alert("البريد الإلكتروني أو كلمة المرور غير صحيحة، حاول مرة أخرى.");
                } else {
                    alert(result.message || "حدث خطأ أثناء تسجيل الدخول.");
                }
            }
        } catch (error) {
            console.error("Fetch error:", error);
            alert("فشل الاتصال بالسيرفر، برجاء المحاولة لاحقاً.");
        }
    });
}