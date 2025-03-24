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
                <tr id="employeeEvent-@employeeEvent.Id">
                    <td>${response.data.employeeFirstName} ${response.data.employeeLastName}</td>
                    <td>${response.data.employeeFavouriteDrink ? response.data.employeeFavouriteDrink : "Not Specified"}</td>
                    <td class="text-end">
                        <button class="btn btn-danger btn-sm deleteEmployeeBtn" data-id="${response.data.id}">Delete</button>
                     </td>
                </tr>
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