function updateBudgetSummary(budgetid) {

        var budget = {
            "BudgetID": budgetid
        }


        $.ajax({
            type: 'POST', //HTTP POST Method
            url: '/Budget/GetBudgetSummary', // Controller/View
            data: JSON.stringify(budget),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (budgetsummary) {
                $('#BudgetSummaryIncome').text(budgetsummary.ExpectedIncome.toString());
                $('#BudgetSummaryExpenditures').text(budgetsummary.Expenditures);
                $('#BudgetSummaryGoals').text(budgetsummary.Goals);
                $('#BudgetSummaryRemaining').text(budgetsummary.Remaining);

            },
            failure: function (response) {
                alert("Fail");
            },
            error: function (response) {
                alert("Fail");
            }
        });
    }