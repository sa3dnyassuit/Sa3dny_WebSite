function openForm(serviceId) {
    document.getElementById("selectedServiceId").value = serviceId;
    document.getElementById("serviceForm").style.display = "block";
}

async function sendRequest() {
    const token = localStorage.getItem('token');
    const customerId = localStorage.getItem('userId');

    // التحقق من تسجيل الدخول
    if (!token || !customerId) {
        alert("برجاء تسجيل الدخول أولاً.");
        window.location.href = 'login.html';
        return;
    }

    const serviceId = document.getElementById("selectedServiceId").value;
    const problem = document.getElementById("problemInput").value.trim();
    const address = document.getElementById("addressInput").value.trim();
    const phone = document.getElementById("phoneInput").value.trim();

    if (!problem || !address || !phone) {
        alert("برجاء ملء كافة البيانات.");
        return;
    }

    const requestData = {
        Customer_Id: parseInt(customerId),
        Service_Id: parseInt(serviceId),
        Description_Req: problem,
        Address: address,
        Phone: phone
    };

    try {
        const response = await fetch('https://recite-slander-riverboat.ngrok-free.dev/api/Requests/create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
                'ngrok-skip-browser-warning': 'true'
            },
            body: JSON.stringify(requestData)
        });

        const result = await response.json();

        if (response.ok) {
            // حفظ بيانات الطلب لعرضها في صفحة النجاح
            localStorage.setItem('requestCode', result.requestCode);
            localStorage.setItem('requestStatus', result.status);
            window.location.href = 'request-success.html';
        } else {
            alert(result.message || "فشل إرسال الطلب، تأكد من اتصال الباك إند.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("خطأ في الاتصال بالسيرفر.");
    }
}