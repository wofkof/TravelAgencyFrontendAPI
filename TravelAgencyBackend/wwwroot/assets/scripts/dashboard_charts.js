document.addEventListener("DOMContentLoaded", function () {
    function tryInitChart(id, config) {
        const ctx = document.getElementById(id);
        if (ctx) new Chart(ctx.getContext("2d"), config);
    }

    tryInitChart("genderChart", {
        type: "pie",
        data: {
            labels: genderLabels,
            datasets: [{
                label: "會員性別",
                data: genderData,
                backgroundColor: [
                    "rgba(75, 192, 192, 0.6)",
                    "rgba(255, 99, 132, 0.6)",
                    "rgba(201, 203, 207, 0.6)"
                ],
                borderColor: "rgba(255,255,255,1)",
                borderWidth: 1
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    tryInitChart("orderChart", {
        type: "bar",
        data: {
            labels: orderLabels,
            datasets: [{
                label: "每月新增訂單數",
                data: orderData,
                backgroundColor: "rgba(255, 159, 64, 0.6)",
                borderColor: "rgba(255, 159, 64, 1)",
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, precision: 0 } }
        }
    });

    tryInitChart("paymentChart", {
        type: "pie",
        data: {
            labels: paymentLabels,
            datasets: [{
                label: "付款方式",
                data: paymentData,
                backgroundColor: [
                    "rgba(153, 102, 255, 0.6)",
                    "rgba(255, 206, 86, 0.6)",
                    "rgba(54, 162, 235, 0.6)"
                ],
                borderColor: "rgba(255,255,255,1)",
                borderWidth: 1
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    tryInitChart("orderStatusChart", {
        type: "pie",
        data: {
            labels: orderStatusLabels,
            datasets: [{
                label: "訂單狀態",
                data: orderStatusData,
                backgroundColor: [
                    "rgba(75, 192, 192, 0.6)",
                    "rgba(255, 205, 86, 0.6)",
                    "rgba(255, 99, 132, 0.6)"
                ],
                borderColor: "rgba(255,255,255,1)",
                borderWidth: 1
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    tryInitChart("memberChart", {
        type: "line",
        data: {
            labels: memberLabels,
            datasets: [{
                label: "每月新增會員數",
                data: memberData,
                borderColor: "rgba(54, 162, 235, 1)",
                backgroundColor: "rgba(54, 162, 235, 0.2)",
                borderWidth: 2,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true } }
        }
    });

    tryInitChart("ratingChart", {
        type: "bar",
        data: {
            labels: ratingLabels,
            datasets: [{
                label: "星級評論數量",
                data: ratingData,
                backgroundColor: "rgba(255, 205, 86, 0.6)",
                borderColor: "rgba(255, 205, 86, 1)",
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, precision: 0 } }
        }
    });

    tryInitChart("travelStatusChart", {
        type: "pie",
        data: {
            labels: travelStatusLabels,
            datasets: [{
                label: "行程狀態",
                data: travelStatusData,
                backgroundColor: [
                    "rgba(75, 192, 192, 0.6)",
                    "rgba(255, 99, 132, 0.6)",
                    "rgba(153, 102, 255, 0.6)"
                ],
                borderColor: "rgba(255,255,255,1)",
                borderWidth: 1
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    tryInitChart("officialTravelStatusChart", {
        type: "pie",
        data: {
            labels: officialTravelStatusLabels,
            datasets: [{
                label: "官方行程狀態",
                data: officialTravelStatusCounts,
                backgroundColor: [
                    "rgba(255, 99, 132, 0.6)",
                    "rgba(255, 205, 86, 0.6)",
                    "rgba(54, 162, 235, 0.6)"
                ],
                borderColor: "rgba(255, 255, 255, 1)",
                borderWidth: 1
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    tryInitChart("officialTravelMonthChart", {
        type: "bar",
        data: {
            labels: officialTravelMonthLabels,
            datasets: [{
                label: "每月新增官方行程數",
                data: officialTravelMonthCounts,
                backgroundColor: "rgba(153, 102, 255, 0.6)",
                borderColor: "rgba(153, 102, 255, 1)",
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { beginAtZero: true, precision: 0 }
            }
        }
    });
});
