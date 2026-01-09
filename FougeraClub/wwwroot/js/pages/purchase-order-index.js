// Ensure signalR lib is loaded in _Layout
var connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

// --- SignalR Events ---
connection.on("ReceiveOTP", function (otp) {
    // Show Toastr Notification (Best Practice for OTP)
    toastr.success(`Your OTP Code is: <b>${otp}</b>`, "OTP Received", {
        timeOut: 10000,
        progressBar: true,
        closeButton: true
    });

    console.log("Debug OTP:", otp);
});

connection.start().catch(err => console.error(err.toString()));

// --- Page Events ---
$(document).ready(function () {

    // Delete Button Logic
    $('.btn-delete').click(function () {
        var id = $(this).data('id');
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                // Perform Delete Request (Ajax or Form Submit)
                // For simplicity, we can redirect or use fetch
                window.location.href = '/PurchaseOrder/Delete/' + id;
            }
        });
    });

    // Sign Process Logic
    $('.btn-sign').click(function () {
        var orderId = $(this).data('id');

        // 1. Trigger OTP Generation
        Swal.fire({
            title: 'Initiating Signature...',
            text: 'Requesting OTP from server...',
            didOpen: () => {
                Swal.showLoading();
                $.post('/PurchaseOrder/GenerateOTP', { orderId: orderId })
                    .done(function (response) {
                        Swal.close();
                        if (response.success) {
                            promptForOtp(orderId);
                        } else {
                            toastr.error(response.message);
                        }
                    })
                    .fail(function () {
                        Swal.fire('Error', 'Failed to generate OTP', 'error');
                    });
            }
        });
    });
});

function promptForOtp(orderId) {
    Swal.fire({
        title: 'Enter Verification Code',
        input: 'text',
        inputLabel: 'Please check your notifications for the OTP code',
        inputPlaceholder: 'e.g. 1234',
        showCancelButton: true,
        confirmButtonText: 'Verify & Sign',
        inputValidator: (value) => {
            if (!value) {
                return 'You need to write something!'
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {
            verifyOtpServer(orderId, result.value);
        }
    });
}

function verifyOtpServer(orderId, otpCode) {
    $.post('/PurchaseOrder/VerifyOTP', { orderId: orderId, otp: otpCode })
        .done(function (res) {
            if (res.success) {
                Swal.fire('Signed!', res.message, 'success').then(() => {
                    // Open Print Page
                    if (res.printUrl) window.open(res.printUrl, '_blank');
                    // Reload to update status
                    location.reload();
                });
            } else {
                Swal.fire('Invalid OTP', res.message, 'error');
            }
        });
}