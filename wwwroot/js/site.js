// Smart Inventory Client Scripts
document.addEventListener("DOMContentLoaded", function () {
    // 1. Auto-dismiss flash alert messages after 5 seconds
    setTimeout(function () {
        var alerts = document.querySelectorAll(".custom-alert");
        alerts.forEach(function (alert) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        });
    }, 5000);

    // 2. Initialize Bootstrap Tooltips if any
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // 3. Dynamic Stock Preview in Stock Adjustment page
    var productSelect = document.getElementById("ProductSelect");
    var adjustmentType = document.getElementById("AdjustmentType");
    var quantityInput = document.getElementById("QuantityInput");
    var previewNewStock = document.getElementById("PreviewNewStock");
    var currentStockDisplay = document.getElementById("CurrentStockDisplay");

    function updateStockPreview() {
        if (!currentStockDisplay || !quantityInput || !adjustmentType) return;
        var current = parseInt(currentStockDisplay.innerText) || 0;
        var qty = parseInt(quantityInput.value) || 0;
        var type = adjustmentType.value;

        var projected = current;
        if (type === "1" || type === "StockIn" || type === "4" || type === "Return") {
            projected = current + qty;
        } else if (type === "2" || type === "StockOut") {
            projected = Math.max(0, current - qty);
        } else if (type === "3" || type === "Adjustment") {
            projected = qty;
        }

        if (previewNewStock) {
            previewNewStock.innerText = projected;
            if (projected === 0) {
                previewNewStock.className = "text-danger fw-bold";
            } else if (projected <= 10) {
                previewNewStock.className = "text-warning fw-bold";
            } else {
                previewNewStock.className = "text-success fw-bold";
            }
        }
    }

    if (quantityInput) quantityInput.addEventListener("input", updateStockPreview);
    if (adjustmentType) adjustmentType.addEventListener("change", updateStockPreview);

    if (productSelect) {
        productSelect.addEventListener("change", function () {
            var productId = this.value;
            if (!productId) return;
            fetch('/Stock/GetProductStockInfo/' + productId)
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data && currentStockDisplay) {
                        currentStockDisplay.innerText = data.quantityInStock;
                        var skuDisplay = document.getElementById("ProductSkuDisplay");
                        if (skuDisplay) skuDisplay.innerText = data.sku;
                        var unitDisplay = document.getElementById("ProductUnitDisplay");
                        if (unitDisplay) unitDisplay.innerText = data.unit;
                        updateStockPreview();
                    }
                })
                .catch(function (err) { console.error("Error fetching stock:", err); });
        });
    }
});
