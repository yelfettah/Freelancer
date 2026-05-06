document.addEventListener("DOMContentLoaded", function () {
    var loadingMap = {
        document: ["Yazışma analiz ediliyor...", "Teklif hazırlanıyor...", "Sözleşme oluşturuluyor...", "Fatura düzenleniyor..."],
        franchise: ["Piyasa analizi yapılıyor...", "Rekabet araştırılıyor...", "Fizibilite hesaplanıyor..."],
        "franchise-compare": ["İlk franchise analiz ediliyor...", "İkinci franchise analiz ediliyor...", "Karşılaştırma yapılıyor...", "En iyi yatırım belirleniyor..."],
        lawyer: ["Yasal tarifeler inceleniyor...", "Dava süreci analiz ediliyor...", "Maliyet hesaplanıyor..."],
        realestate: ["Bölge fiyatları araştırılıyor...", "Karşılaştırma yapılıyor...", "İlan metni yazılıyor..."],
        "realestate-compare": ["İlk mülk değerleniyor...", "İkinci mülk değerleniyor...", "Karşılaştırma hazırlanıyor...", "Sonuçlar derleniyor..."],
        sme: ["Piyasa fiyatları kontrol ediliyor...", "Maliyet hesaplanıyor...", "Teklif hazırlanıyor..."]
    };

    var form = document.querySelector("form[data-loading-kind]");
    var overlay = document.getElementById("loadingOverlay");
    var loadingText = document.getElementById("loadingText");
    var intervalId;
    if (form && overlay && loadingText) {
        var messages = loadingMap[form.getAttribute("data-loading-kind")] || loadingMap.document;
        var messageIndex = 0;
        form.addEventListener("submit", function () {
            overlay.classList.add("show");
            intervalId = setInterval(function () {
                loadingText.style.opacity = "0";
                setTimeout(function () {
                    messageIndex = (messageIndex + 1) % messages.length;
                    loadingText.textContent = messages[messageIndex];
                    loadingText.style.opacity = "1";
                }, 180);
            }, 2200);
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
                setTimeout(function () { button.textContent = original; }, 2000);
            });
        });
    });

    var rawToggle = document.getElementById("rawToggle");
    var chatToggle = document.getElementById("chatToggle");
    var textArea = document.getElementById("rawTextArea");
    var chatPreview = document.getElementById("chatPreview");
    function parseChat() {
        if (!textArea || !chatPreview) return;
        var lines = textArea.value.split("\n").filter(function (l) { return l.trim().length > 0; });
        var regex = /^\[?(\d{1,2}[:.]\d{2})\]?\s*(.+?):\s*(.+)$/;
        var firstName = "";
        chatPreview.innerHTML = "";
        lines.forEach(function (line) {
            var match = line.match(regex);
            if (!match) return;
            var time = match[1];
            var name = match[2].trim();
            var msg = match[3].trim();
            if (!firstName) firstName = name;
            var isLeft = name === firstName;
            var bubble = document.createElement("div");
            bubble.className = isLeft ? "chat-bubble-left" : "chat-bubble-right";
            bubble.innerHTML = '<div class="chat-name">' + name + '</div><div>' + msg + '</div><div class="chat-time">' + time + '</div>';
            chatPreview.appendChild(bubble);
        });
    }
    if (chatToggle && rawToggle && textArea && chatPreview) {
        chatToggle.addEventListener("click", function () {
            parseChat();
            textArea.classList.add("d-none");
            chatPreview.classList.remove("d-none");
            chatToggle.classList.add("active");
            rawToggle.classList.remove("active");
        });
        rawToggle.addEventListener("click", function () {
            textArea.classList.remove("d-none");
            chatPreview.classList.add("d-none");
            rawToggle.classList.add("active");
            chatToggle.classList.remove("active");
        });
        textArea.addEventListener("input", parseChat);
    }

    var parameterContainer = document.getElementById("parameterContainer");
    var addParameter = document.getElementById("addParameter");
    function reindexParameters() {
        if (!parameterContainer) return;
        Array.from(parameterContainer.querySelectorAll(".parameter-row")).forEach(function (row, index) {
            var name = row.querySelector(".parameter-name");
            var value = row.querySelector(".parameter-value");
            var description = row.querySelector(".parameter-description");
            if (name) name.name = "Parameters[" + index + "].Name";
            if (value) value.name = "Parameters[" + index + "].Value";
            if (description) description.name = "Parameters[" + index + "].Description";
        });
    }
    if (addParameter && parameterContainer) {
        addParameter.addEventListener("click", function () {
            var row = document.createElement("div");
            row.className = "row g-2 mb-2 parameter-row";
            row.innerHTML = '<div class="col-md-4"><input type="text" class="form-control parameter-name" placeholder="Parametre Adı"></div><div class="col-md-4"><input type="text" class="form-control parameter-value" placeholder="Değer"></div><div class="col-md-3"><input type="text" class="form-control parameter-description" placeholder="Açıklama"></div><div class="col-md-1"><button type="button" class="btn btn-outline-danger w-100 remove-parameter">X</button></div>';
            parameterContainer.appendChild(row);
            reindexParameters();
        });
        parameterContainer.addEventListener("click", function (event) {
            if (event.target.classList.contains("remove-parameter")) {
                var row = event.target.closest(".parameter-row");
                if (row) row.remove();
                reindexParameters();
            }
        });
        var firstRow = parameterContainer.querySelector(".row");
        if (firstRow) firstRow.classList.add("parameter-row");
        reindexParameters();
    }

    var jobCards = document.querySelectorAll(".job-type-card");
    var businessInput = document.getElementById("selectedBusinessType");
    jobCards.forEach(function (card, index) {
        card.addEventListener("click", function () {
            jobCards.forEach(function (c) { c.classList.remove("selected"); });
            card.classList.add("selected");
            if (businessInput) businessInput.value = card.getAttribute("data-value") || "";
        });
        if (index === 0) {
            card.classList.add("selected");
            if (businessInput) businessInput.value = card.getAttribute("data-value") || "";
        }
    });

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("fade-in-up");
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });
    document.querySelectorAll(".staggered-card, .card, .result-card, .create-header, .form-section").forEach(function (el) {
        observer.observe(el);
    });

    function colorComparisonRows(tableId) {
        var table = document.getElementById(tableId);
        if (!table) return;
        Array.from(table.querySelectorAll("tbody tr")).forEach(function (row) {
            var mode = row.getAttribute("data-better");
            if (!mode) return;
            var cells = row.querySelectorAll("td");
            if (cells.length < 3) return;
            var left = parseFloat(cells[1].getAttribute("data-value"));
            var right = parseFloat(cells[2].getAttribute("data-value"));
            if (Number.isNaN(left) || Number.isNaN(right)) return;
            cells[1].classList.remove("better-value", "worse-value", "neutral-value");
            cells[2].classList.remove("better-value", "worse-value", "neutral-value");
            if (left === right) {
                cells[1].classList.add("neutral-value");
                cells[2].classList.add("neutral-value");
                return;
            }
            var leftBetter = mode === "lower" ? left < right : left > right;
            cells[1].classList.add(leftBetter ? "better-value" : "worse-value");
            cells[2].classList.add(leftBetter ? "worse-value" : "better-value");
        });
    }

    colorComparisonRows("franchiseComparisonTable");
    colorComparisonRows("realEstateComparisonTable");

    window.addEventListener("beforeunload", function () {
        if (intervalId) clearInterval(intervalId);
    });
});
