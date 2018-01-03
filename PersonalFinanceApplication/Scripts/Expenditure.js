//Variables that need to be public to the whole js file
var budget = { "BudgetID": null };
var categories;
var pieChart;

//Call a get method to return all the BudgetCategories (budgetcategories) from the server
function getExpenditures(budgetid)
{
    budget.BudgetID = budgetid
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/Expenditures', // Controller/View
        data: JSON.stringify(budget),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            categories = JSON.parse(response);
            for (var i = 0; i < categories.length; i++) {
                var category = categories[i];
                //editCategoryHtml(category);
            }
            //Create the piechart
            addPieChart(categories);

            //Show all transactions
            showTransactions(categories[0].Transactions, categories[0].CategoryName);

            //Add the categories to the category tab
            addCategories();
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function updateBudgetCategory(categoryid, amount) {
    var budgetcategory = {
        "BudgetID": budget.BudgetID,
        "CategoryID": categoryid,
        "Amount": amount
    }


    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/UpdateCategory', // Controller/View
        data: JSON.stringify(budgetcategory),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            alert("Category Successfully Updated");
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function deleteBudgetCategory(categoryid) {

    var budgetcategory = {
        "BudgetID": budget.BudgetID,
        "CategoryID": categoryid,
    }
    
    $.ajax({
        type: 'POST', //HTTP GET Method
        url: '/Budget/DeleteCategory', // Controller/View
        data: JSON.stringify(budgetcategory),
        dataType: "json",
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            alert("Category Successfully Deleted");
        },
        failure: function (response) {
            alert("Failure");
        },
        error: function (response) {
            alert("Error");
        }
    });
}

function addCategory(category) {

    var html = "<div class=\"categoryRow " + category.CategoryID + "\">" +
                    "<div class=\"row\">" +
                        "<div class=\"col-md-2\">" +
                            category.CategoryName +
                        "</div>" +
                        "<div class=\"col-md-2\">" +
                            "<input id = \"Amount" + category.CategoryID + "\" type=\"number\" class=\"form-control categoryamountinput " + category.CategoryName + "\" value=\"" + category.Amount + "\" placeholder=\"" + category.Amount + "\" />" +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            category.BudgetType +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            "<button id = \"Update" + category.CategoryID + "\" class=\"" + category.CategoryID + " btn btn-success categoryupdate\">Update</button>" +
                        "</div>" +
                        "<div class=\"col-md-1\">" +
                            "<button id = \"Remove" + category.CategoryID + "\" class= \"" + category.CategoryID + " btn btn-danger categoryremove\">Remove</button>" +
                        "</div>" +
                    "</div>" +
                "<hr />" +
                "</div>"; 

    $('#categories').prepend(html);

    $("#Update" + category.CategoryID).on("click", function () {
        var categoryid = this.classList[0];
        

        var category = categories.find(function (obj) { return obj.CategoryID == categoryid; });
        
        category.Amount = $("#Amount" + category.CategoryID).val();
        
        updateBudgetCategory(category.CategoryID, category.Amount);
    });

    $("#Remove" + category.CategoryID).on("click", function () {
        var categoryid = this.classList[0];
        //alert(this.classList[0]);

        //Hide the row of the removed category
        $('.categoryrow, .' + categoryid).hide();
        //remove the 
        var category = categories.find(function (obj) { return obj.CategoryID == categoryid; });
        categories.splice(categories.indexOf(category, 1));
        
        deleteBudgetCategory(category.CategoryID);

        removeFromPieChart(category);

        $('.transactionrow.' + category.CategoryName.replace(/\s/g, '')).addClass("Other"); 
    });
}

function addCategories() {
    //Remove the first element of the categories array (which contains totals) since we don't want totals displayed as a category
    var allCategories = categories.slice(2);

    for (var i = 0; i < allCategories.length; i++) {
        addCategory(allCategories[i]);
    }
}


//Append the transactions to the transactions div
function showTransactions(transactions, categoryname) {
    for (var i = 0; i < transactions.length; i++)
    {
        //For each transaction in the array, create a row and append it to the transactions div
        var html = "<div class=\"transactionrow " + transactions[i].Tag.replace(/\s/g, '') + " " + transactions[i].CategoryName.replace(/\s/g, '') + "\">" +
                    "<div class=\"row\">" +
                        "<div class=\"col-md-2 transactiondate " + categoryname + "\">" +
                            transactions[i].DateString +
                        "</div>" + 
                        "<div class=\"col-md-4 transactiondesc " + categoryname + "\">" +
                            transactions[i].Description +
                        "</div>" + 
                        "<div class=\"col-md-2 transactionamount " + categoryname + "\">" +
                            transactions[i].Amount + 
                        "</div>" + 
                        "<div class=\"col-md-2 transactioncategory " + categoryname + "\">" +
                            transactions[i].CategoryName + 
                        "</div>" +
                    "</div><hr /></div>";
        $('#transactions').append(html);
    }
}

//Clear the transactions div
function clearTransactions() {
    //Clear the transactions div of previous transactions
    //$('#transactions').html("");
    
}

function addPieChart(categories) {
    //Remove the first element of the categories array (which contains totals) since we don't want totals in the piechart
    var pieCategories = categories.slice();
    pieCategories.shift();

    //Create an array of category names
    var categoryNames = pieCategories.map(c => c.CategoryName);

    //Convert all the 'used amounts' passed from the server into a json object
    var usedAmounts = pieCategories.map(c => c.UsedAmount);

    //Create an array of random colors the same size as the number of categories in the budget
    var colors = [];
    for (var i = 0; i < usedAmounts.length; i++) {
        colors.push(getRandomColor());
    }

    //Add the values, labels, and colors into a data object to be used by the piechart
    var data = {
        labels: categoryNames,
        datasets: [{
            data: usedAmounts,
            backgroundColor: colors
        }]
    };

    //create the elements necessary to create a pie chart on on the 'piecanvas' div
    var canvas = document.getElementById("piecanvas");
    var ctx = canvas.getContext("2d");
    pieChart = new Chart(ctx, {
        type: 'pie',
        data: data,
    });

    /*When a pie slice is clicked, pass the name of the category to the create donut chart
    function in order to create a donut chart of the used and remaining amounts for that category */
    canvas.onclick = function (evt) {
        clearTransactions();
        var activePoints = pieChart.getElementsAtEvent(evt);
        if (activePoints[0]) {
            var chartData = activePoints[0]['_chart'].config.data;
            var idx = activePoints[0]['_index'];
            var label = chartData.labels[idx].replace(/\s/g, ''); //Removes spaces in category names to match class name

            //Hide all transaction rows and then show transaction rows associated with the category clicked.
            $('.transactionrow').hide();
            $('.' + label).show();
        }
    };
}

function addToPieChart(category) {
    pieChart.data.labels.push(category.CategoryName);
    pieChart.data.datasets.forEach((dataset) => {
        dataset.data.push(category.UsedAmount);
    });
    pieChart.data.datasets.forEach((dataset) => {
        dataset.backgroundColor.push(getRandomColor());
    });
    pieChart.update();
}

function removeFromPieChart(category) {
    var index = pieChart.data.labels.indexOf(category.CategoryName);

    if (index > -1)
    {
        pieChart.data.labels.splice(index, 1);
        pieChart.data.datasets.forEach((dataset) => {
            dataset.data.splice(index, 1);
        });
        pieChart.data.datasets.forEach((dataset) => {
            dataset.backgroundColor.splice(index, 1);
        });
        pieChart.update();
    }
    
}

//This function generates a random hex color and is used to randomly color different slices of the pie chart
function getRandomColor() {
    var letters = '0123456789ABCDEF'.split('');
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

$(document).ready(function () {

    $('#addCategoryBtn').click(function () {
        var newCategory = {
            "BudgetID": budget.BudgetID,
            "CategoryID": $('#newCategoryName').val(),
            "Amount": $('#newCategoryAmount').val(),
            "BudgetTypeID": $('#newCategoryType').val(),
        }

        $.ajax({
            type: 'POST', //HTTP GET Method
            url: '/Budget/AddCategory', // Controller/View
            data: JSON.stringify(newCategory),
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                var successalert = "Category Successfully added\n" +
                    "\nCategory: " + response.CategoryName +
                    "\nAmount: " + response.Amount +
                    "\nType: " + response.BudgetType;
                //alert(successalert);
                categories.push(response);
                addCategory(response);
                
                $('#newCategoryAmount').val('');
                $('#newCategoryName').val('');
                $('#newCategoryType').val('');
                addToPieChart(response);

                //Remove the other class on all transactions corresponsding with added cateogory so they don't show up when clickin the other slice
                $('.transactionrow.' + response.CategoryName.replace(/\s/g, '')).removeClass("Other");                
            },
            failure: function (response) {
                alert("Failure");
            },
            error: function (response) {
                alert("Error");
            }
        });
    });
});