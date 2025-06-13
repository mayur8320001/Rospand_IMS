
$(document).ready(function () {
    // Example: Call API on button click
    $('#fetchDataButton').click(function () {
        $.ajax({
            url: '/api/some-endpoint', // Replace with your API endpoint
            type: 'GET', // Specify HTTP method (GET, POST, etc.)
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                console.log('Data received:', data);
                // Handle response (e.g., update UI)
                $('#dataContainer').html(JSON.stringify(data));
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                if (xhr.status === 401) {
                    alert('Unauthorized. Please log in again.');
                    window.location.href = '/Auth/Login';
                }
            }
        });
    });
});