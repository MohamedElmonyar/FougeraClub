$(document).ready(function () {
    // 1. Initialize Calculations on Load
    recalculateAll();

    // 2. Add New Row Event
    $('#btnAddRow').click(function () {
        var container = $('#itemsContainer');
        var index = container.find('tr').length; // Calculate new index

        // Get template and replace placeholder with actual index
        var template = $('#rowTemplate').html();
        var newRowHtml = template.replace(/INDEX_PLACEHOLDER/g, index);

        container.append(newRowHtml);
    });

    // 3. Remove Row Event (Delegated)
    $('#itemsContainer').on('click', '.btn-remove-row', function () {
        $(this).closest('tr').remove();
        reindexRows(); // Fix indexes for Model Binding
        recalculateAll();
    });

    // 4. Input Change Events (Delegated for dynamic rows)
    $('#itemsContainer').on('input', '.input-qty, .input-price', function () {
        var row = $(this).closest('tr');
        calculateRowTotal(row);
        recalculateAll();
    });

    // 5. Tax Switch Event
    $('#taxSwitch').change(function () {
        recalculateAll();
    });
});

function calculateRowTotal(row) {
    var qty = parseFloat(row.find('.input-qty').val()) || 0;
    var price = parseFloat(row.find('.input-price').val()) || 0;
    var total = qty * price;
    row.find('.input-total').val(total.toFixed(2));
}

function recalculateAll() {
    var subTotal = 0;

    // Sum up all rows
    $('.input-total').each(function () {
        subTotal += parseFloat($(this).val()) || 0;
    });

    var hasTax = $('#taxSwitch').is(':checked');
    var taxAmount = hasTax ? (subTotal * 0.05) : 0;
    var netTotal = subTotal + taxAmount;

    // Update UI Labels
    $('#lblSubTotal').text(subTotal.toFixed(2));
    $('#lblTax').text(taxAmount.toFixed(2));
    $('#lblNetTotal').text(netTotal.toFixed(2));
}

function reindexRows() {
    // Re-assign correct indexes (0, 1, 2...) so ASP.NET Core can bind the list
    $('#itemsContainer tr').each(function (i, row) {
        $(row).find('input, select, span').each(function () {
            // Update 'name' attribute
            if (this.name) {
                this.name = this.name.replace(/Items\[\d+\]/, 'Items[' + i + ']');
            }
            // Update validation attributes
            var valFor = $(this).attr('data-valmsg-for');
            if (valFor) {
                $(this).attr('data-valmsg-for', valFor.replace(/Items\[\d+\]/, 'Items[' + i + ']'));
            }
        });
    });
}