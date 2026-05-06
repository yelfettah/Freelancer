document.addEventListener("DOMContentLoaded", function () {
    var form = document.querySelector("form");
    var loadingOverlay = document.querySelector(".loading-overlay");
    var loadingMessage = document.querySelector(".loading-message");
    var loadingTexts = [
        "Yazışma analiz ediliyor...",
        "Teklif hazırlanıyor...",
        "Sözleşme oluşturuluyor...",
        "Fatura düzenleniyor..."
    ];
    var loadingIndex = 0;
    var loadingIntervalId = null;

    if (form && loadingOverlay) {
        form.addEventListener("submit", function () {
            loadingOverlay.classList.add("show");

            if (loadingMessage) {
                loadingMessage.textContent = loadingTexts[0];
                loadingIntervalId = setInterval(function () {
                    loadingIndex = (loadingIndex + 1) % loadingTexts.length;
                    loadingMessage.textContent = loadingTexts[loadingIndex];
                }, 3000);
            }
        });
    }

    var copyButtons = document.querySelectorAll(".btn-copy");
    copyButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            var targetSelector = button.getAttribute("data-target");
            var targetElement = targetSelector ? document.querySelector(targetSelector) : null;
            var textToCopy = targetElement ? targetElement.value || targetElement.textContent : "";
            var originalText = button.textContent;

            if (!textToCopy) {
                return;
            }

            navigator.clipboard.writeText(textToCopy).then(function () {
                button.textContent = "Kopyalandı ✓";
                setTimeout(function () {
                    button.textContent = originalText;
                }, 2000);
            });
        });
    });

    var autoResizeTextarea = document.querySelector("#conversationTextarea");
    if (autoResizeTextarea) {
        var resize = function () {
            autoResizeTextarea.style.height = "auto";
            autoResizeTextarea.style.height = autoResizeTextarea.scrollHeight + "px";
        };

        autoResizeTextarea.addEventListener("input", resize);
        resize();
    }

    window.addEventListener("beforeunload", function () {
        if (loadingIntervalId) {
            clearInterval(loadingIntervalId);
        }
    });
});
