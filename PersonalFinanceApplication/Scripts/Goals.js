//Variables that need to be public to the whole js file
var budget = { "BudgetID": null };
var goals;

function getGoals(budgetid) {
    budget.BudgetID = budgetid
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/GetGoals', // Controller/View
        data: JSON.stringify(budget),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            goals = JSON.parse(response);
            initalizeAllGoals();
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}
function initalizeAllGoals() {
    for (var i = 0; i < goals.length; i++)
    {
        addGoalHtml(goals[i]);
        generateGoalDonutChart(goals[i]);
    }
}

function generateGoalDonutChart(goal) {
    //Create a donut chart and append it to the associated goal object

    //Create an array of labels
    var goallabels = ["Saved", "Remaining"];

    //Create array of data
    var savedamount = goal.CurrentAmount - goal.BeginningAmount;
    var remainingamount = goal.EndAmount - goal.CurrentAmount;
    if (remainingamount > 0) {
        var goalvalues = [savedamount, remainingamount ]
    }
    else {
        var goalvalues = [savedamount, 0]
    }

    //Create an array of colors
    var goalcolors = ["#37a848", "#9d9fa0"]; //green for unused and gray for used

    //Add the values, labels, and colors into a data object to be used by the piechart
    var data = {
        labels: goallabels,
        datasets: [{
            data: goalvalues,
            backgroundColor: goalcolors
        }]
    };
    
    //create the elements necessary to create a pie chart on on the 'piecanvas' div
    var ctx = document.getElementById('goalcanvas' + goal.GoalID).getContext("2d");
    goal.DonutChart = new Chart(ctx, {
        type: 'doughnut',
        data: data,
    }); 
}

function addGoalHtml(goal) {
    var html =  "<div id=\"goaldiv" + goal.GoalID + "\" class=\"col-md-4\" style=\"text-align: center\">" +
                    "<h3>" + goal.GoalName + "</h3>" +
                    "<canvas id=\"goalcanvas" + goal.GoalID + "\"></canvas>" +
                    "<br />" +
                    "<p>" + goal.Description + "</p>" + 
                    "<br />" +
                    "<div class=\"row\">" +
                        "<div class=\"col-md-6\">" +
                            "<strong>Current Amount: </strong>" + 
                        "</div>" +
                        "<div class=\"col-md-6\">" +
                            "<input type=\"number\" class=\"form-control\" id=\"currentamount" + goal.GoalID + "\" value=\"" + goal.CurrentAmount + "\" />" + 
                        "</div>" +
                    "</div>" +
                    "<div class=\"row\">" +
                        "<div class=\"col-md-6\">" +
                            "<strong>End Amount: </strong>" +
                        "</div>" +
                        "<div class=\"col-md-6\">" +
                            "<input type=\"number\" class=\"form-control\" id=\"endamount" + goal.GoalID + "\" value=\"" + goal.EndAmount + "\" />" +
                        "</div>" +
                    "</div>" +
                    "<br />" +
                    "<br />" +
                    "<button id=\"updategoalbtn" + goal.GoalID + "\" class=\"btn btn-success\">Update</button>" + "  " +
                    "<button id=\"deletegoalbtn" + goal.GoalID + "\" class=\"btn btn-danger\">Delete</button>" +
                    "<br />" +
                    "<br />" +
                    "<br />" +
                "</div>";

    $('#goals').prepend(html);

    $("#updategoalbtn" + goal.GoalID).on("click", function () {
        var newgoalid = this.id.match(/\d+/)[0];
        var newgoal = {
            "GoalID": newgoalid,
            "CurrentAmount": $('#currentamount' + newgoalid).val(),
            "EndAmount": $('#endamount' + newgoalid).val(),
        }

        updateGoal(newgoal);
    });

    $("#deletegoalbtn" + goal.GoalID).on("click", function () {
        var deletegoalid = this.id.match(/\d+/)[0];
        var deletegoal = {
            "GoalID": deletegoalid,
        }

        deleteGoal(deletegoal);
    });
}

function addGoal(goal)
{
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/AddGoal', // Controller/View
        data: JSON.stringify(goal),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            goals.push(response);
            addGoalHtml(response);
            generateGoalDonutChart(response);

            $('#GoalNameInput').val('');
            $('#GoalDescriptionInput').val('');
            $('#GoalBegAmountInput').val('');
            $('#GoalEndAmountInput').val('');

        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function updateGoal(goal) {
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/UpdateGoal', // Controller/View
        data: JSON.stringify(goal),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            var goal = goals.find(function (obj) { return obj.GoalID == response.GoalID; });
            var index = goals.indexOf(goal);
            goal[index] = response;

            generateGoalDonutChart(goal[index]);

        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function deleteGoal(goal) {
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/DeleteGoal', // Controller/View
        data: JSON.stringify(goal),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            //alert("success");
            var goal = goals.find(function (obj) { return obj.GoalID == response.GoalID; });
            goals.splice(goals.indexOf(goal, 1));
            $('#goaldiv' + response.GoalID).hide();
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

    $('#addgoalbtn').click(function () {
        var newgoal = {
            "GoalName": $('#GoalNameInput').val(),
            "Description": $('#GoalDescriptionInput').val(),
            "BudgetID": budget.BudgetID,
            "BeginningAmount": $('#GoalBegAmountInput').val(),
            "EndAmount": $('#GoalEndAmountInput').val(),
            "CurrentAmount": $('#GoalBegAmountInput').val()
        }
        addGoal(newgoal);
    });
    
})