document.addEventListener("DOMContentLoaded", function () {
    var form = document.getElementById("generateForm");
    var overlay = document.getElementById("loadingOverlay");
    var loadingText = document.getElementById("loadingText");
    var messages = [
        "Yazışma analiz ediliyor...",
        "Teklif hazırlanıyor...",
        "Sözleşme oluşturuluyor...",
        "Fatura düzenleniyor...",
        "Son rötuşlar yapılıyor..."
    ];
    var messageIndex = 0;
    var intervalId;

    if (form && overlay && loadingText && !window.__ffCreateScriptBound) {
        form.addEventListener("submit", function () {
            overlay.classList.add("show");

            intervalId = setInterval(function () {
                loadingText.style.opacity = "0";
                setTimeout(function () {
                    messageIndex = (messageIndex + 1) % messages.length;
                    loadingText.textContent = messages[messageIndex];
                    loadingText.style.opacity = "1";
                }, 180);
            }, 2500);
        });
    }

    var copyButtons = document.querySelectorAll(".btn-copy");
    copyButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            var targetSelector = button.getAttribute("data-target");
            var target = targetSelector ? document.querySelector(targetSelector) : null;
            if (!target) return;

            var text = target.value || target.textContent || "";
            var original = button.textContent;

            navigator.clipboard.writeText(text).then(function () {
                button.textContent = "Kopyalandı ✓";
                setTimeout(function () {
                    button.textContent = original;
                }, 2000);
            });
        });
    });

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("fade-in-up");
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    document.querySelectorAll(".card, .result-card, .create-header, .form-section").forEach(function (el) {
        observer.observe(el);
    });

    window.addEventListener("beforeunload", function () {
        if (intervalId) clearInterval(intervalId);
    });
});
