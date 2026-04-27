function openServiceForm(serviceName) {
    const overlay = document.getElementById("serviceFormOverlay");
    const title = document.getElementById("selectedServiceTitle");
    const serviceInput = document.getElementById("serviceName");
    const form = document.getElementById("serviceRequestForm");

    if (form) form.reset();

    if (overlay) overlay.style.display = "flex";
    if (title) title.innerText = serviceName + " Request";
    if (serviceInput) serviceInput.value = serviceName;
}

function closeServiceForm() {
    const overlay = document.getElementById("serviceFormOverlay");
    const form = document.getElementById("serviceRequestForm");

    if (form) form.reset();
    if (overlay) overlay.style.display = "none";
}

function generateRequestId() {
    return "SA3DNY" + Math.floor(1000 + Math.random() * 9000);
}

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("serviceRequestForm");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const service = document.getElementById("serviceName").value.trim();
        const problem = document.getElementById("problemDetails").value.trim();
        const address = document.getElementById("address").value.trim();
        const phone = document.getElementById("phone").value.trim();

        if (!problem || !address || !phone) {
            alert("Please fill all fields");
            return;
        }

        const requestData = {
            requestId: generateRequestId(),
            service: service,
            problem: problem,
            address: address,
            phone: phone,
        };

        // نخزن في الاتنين
        const dataString = JSON.stringify(requestData);
        sessionStorage.setItem("requestData", dataString);
        localStorage.setItem("requestData", dataString);

        const isLoggedIn = sessionStorage.getItem("isLoggedIn");

        if (isLoggedIn === "true") {
            window.location.href = "./request-success.html";
        } else {
            window.location.href = "./login.html";
        }
    });
});

window.addEventListener("pageshow", function () {
    const form = document.getElementById("serviceRequestForm");
    const overlay = document.getElementById("serviceFormOverlay");

    if (form) form.reset();
    if (overlay) overlay.style.display = "none";
});
