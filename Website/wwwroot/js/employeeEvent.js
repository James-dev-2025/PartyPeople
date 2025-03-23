$(document).ready(function () {

    $(document).on('click', '.deleteEmployeeBtn', function () {
        var employeeId = $(this).data('id'); 
        deleteEmployeeEvent(employeeId);
    });

    window.createEmployeeEvent = function () {
        console.log("here")
        var eventId = $('input[name="EventId"]').val();
        var employeeId = $('#EmployeeId').val();

        if (!employeeId) {
            alert('Please select an employee');
            return;
        }

        $.ajax({
            url: '/EmployeeEvent/Create',
            method: 'POST',
            data: { EmployeeEvent: { EventId: eventId, EmployeeId: employeeId } },
            success: function (response) {
                console.log(response)
                if (response.success) {
                        
                    var newEmployeeHtml = `
                        <li id="employeeEvent-${response.data.id}">
                            <div class="d-flex flex-row gap-2  align-items-baseline">
                                <p>${response.data.employeeFirstName} ${response.data.employeeLastName}</p>
                                <button class="btn btn-danger btn-sm deleteEmployeeBtn" data-id="${response.data.id}">Delete</button>
                            </div>
                        </li>
                    `;
                    $('#employeeEventList').append(newEmployeeHtml);
                    $('#employeeSearch').val('').trigger('change');
                    $('#EmployeeId').val("")
                    $('#createEmployeeEventBtn').prop("disabled", response.data.eventIsAtCapacity ? "true" : "false")
                } else {
                    alert('Failed to add employee to event');
                }
            },
            error: function () {
                alert('Error while adding employee');
            }
        });
    }

    function deleteEmployeeEvent(id) {
        $.ajax({
            url: '/EmployeeEvent/Delete',
            method: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    // Remove the deleted employee from the list
                    $('#employeeEvent-' + id).remove();
                    $('#createEmployeeEventBtn').prop("disabled", false);
                } else {
                    alert('Failed to delete employee from event');
                }
            },
            error: function () {
                alert('Error while deleting employee');
            }
        });
    }
});