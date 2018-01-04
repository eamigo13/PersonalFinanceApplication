//Variables that need to be public to the whole js file
var budget = { "BudgetID": null };
var incomes;

function getIncomes(budgetid) {
    budget.BudgetID = budgetid
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/GetIncomes', // Controller/View
        data: JSON.stringify(budget),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            incomes = JSON.parse(response);
            initializeAllIncomes();
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function initializeAllIncomes() {
    for (var i = 0; i < incomes.length; i++)
    {
        addIncomeHTML(incomes[i]);
    }
}

function addIncomeHTML(income) {
    var html =  "<div id=\"incomediv" + income.IncomeID + "\" >" +
                    "<div class=\"row\">" +
                        "<div class=\"col-md-2\">" +
                            income.IncomeName + 
                        "</div>" +
                        "<div class=\"col-md-2\">" +
                            "<input type=\"number\" id=\"wage" + income.IncomeID + "\" value=\"" + income.Wage + "\" placeholder=\"" + income.Wage + "\" class=\"form-control\" />" +
                        "</div>" +
                        "<div class=\"col-md-2\">" +
                            "<input type=\"number\" id=\"hours" + income.IncomeID + "\" value=\"" + income.HoursPerWeek + "\" placeholder=\"" + income.HoursPerWeek + "\" class=\"form-control\" />" +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            income.IncomeTypeDescription +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            "<button id=\"updateincomebtn" + income.IncomeID + "\" class=\"btn btn-success\">Update</button>" +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            "<button id=\"deleteincomebtn" + income.IncomeID + "\" class=\"btn btn-danger\">Delete</button>" +
                        "</div>" +
                    "</div>" +
                "<hr />" +
                "</div>";

    $('#incomes').prepend(html);

    $("#updateincomebtn" + income.IncomeID).on("click", function () {
        var newincomeid = this.id.match(/\d+/)[0];
        var newincome = {
            "IncomeID": newincomeid,
            "Wage": $('#wage' + newincomeid).val(),
            "HoursPerWeek": $('#hours' + newincomeid).val(),
        }

        updateIncome(newincome);
    });

    $("#deleteincomebtn" + income.IncomeID).on("click", function () {
        var deleteincomeid = this.id.match(/\d+/)[0];
        var deleteincome = {
            "IncomeID": deleteincomeid,
        }

        deleteIncome(deleteincome);
    });
}

function addIncome(income) {
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/AddIncome', // Controller/View
        data: JSON.stringify(income),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            incomes.push(response);
            addIncomeHTML(response);

            $('#newIncomeType').val('');
            $('#newWageAmount').val('');
            $('#newHoursAmount').val('');
            $('#newIncomeName').val('');

        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function updateIncome(income) {
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/UpdateIncome', // Controller/View
        data: JSON.stringify(income),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            var income = incomes.find(function (obj) { return obj.IncomeID == response.IncomeID; });
            var index = incomes.indexOf(income);
            incomes[index] = response;
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function deleteIncome(income) {
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/DeleteIncome', // Controller/View
        data: JSON.stringify(income),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            $('#incomediv' + response.IncomeID).hide();
            var income = income.find(function (obj) { return obj.IncomeID == response.IncomeID; });
            incomes.splice(incomes.indexOf(income, 1));
            
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

$(document).ready(function () {

    $('#addincomebtn').click(function () {
        var newincome = {
            "IncomeName": $('#newIncomeName').val(),
            "IncomeTypeID": $('#newIncomeType').val(),
            "Wage": $('#newWageAmount').val(),
            "HoursPerWeek": $('#newHoursAmount').val(),
            "BudgetID": budget.BudgetID
        }
        addIncome(newincome);
    });

})