function updateBudgetSummary() {
        $.ajax({
            type: 'POST', //HTTP POST Method
            url: '/Budget/GetBudgetSummary', // Controller/View
            //data: JSON.stringify(budget),
            //dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                var amount = JSON.parse(response);
                var absamount = Math.abs(amount);
                if (amount < 0)
                {
                    var budgettext = "$" + absamount + " over budget";
                    $('#budgetsummarydiv').css('background-color', 'darkred');
                }
                else
                {
                    var budgettext = "$" + amount + " under budget";
                    $('#budgetsummarydiv').css('background-color', 'darkgreen');
                }
                $('#budgetsummary').text(budgettext);
            },
            failure: function (response) {
                alert("Fail");
            },
            error: function (response) {
                alert("Fail");
            }
        });
    }