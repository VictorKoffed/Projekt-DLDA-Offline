/**
 * Renders a comparative bar chart for longitudinal patient progress tracking.
 * Visualizes the delta between a patient's historical and current assessments 
 * to instantly identify clinical trajectories (improvement vs. deterioration).
 */
function renderImprovementChart(labels, previousData, currentData) {
    const ctx = document.getElementById('improvementChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Tidigare',
                    data: previousData,
                    backgroundColor: '#ffc107'
                },
                {
                    label: 'Nuvarande',
                    data: currentData,
                    backgroundColor: '#198754'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4,
                    ticks: { stepSize: 1 },
                    title: {
                        display: true,
                        text: 'Skattning (lägre = bättre)'
                    }
                }
            }
        }
    });
}

/**
 * Toggles the visibility of detailed tabular data.
 * Used to manage cognitive load, allowing patients/staff to digest high-level chart metrics 
 * before drilling down into itemized assessment specifics.
 */
function toggleDetailsPatient() {
    const section = document.getElementById('detailsSection');
    const button = document.getElementById('toggleButton');

    if (!section || !button) return;

    const isVisible = section.style.display === 'block';
    section.style.display = isVisible ? 'none' : 'block';
    button.innerText = isVisible ? '🔍 Visa detaljer per fråga' : '🔜 Dölj detaljer';
}

/**
 * Visualizes the distribution of patient self-assessment scores for a single evaluation.
 * Acts as a rapid clinical triage tool to quickly assess the severity proportion of a patient's condition.
 */
function renderPatientSinglePieChart(labels, data) {
    const ctx = document.getElementById('resultPie');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: ['#198754', '#ffc107', '#dee2e6'],
                borderWidth: 1
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}:${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: {
                        weight: 'bold',
                        size: 16
                    },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

/**
 * Preserves viewport scroll position across synchronous form postbacks.
 * Critical for maintaining staff context when updating individual items in a lengthy clinical assessment list.
 */
function rememberScroll(id) {
    sessionStorage.setItem('scrollTo', id);
}

window.addEventListener('load', function () {
    const lastId = sessionStorage.getItem('scrollTo');
    if (lastId) {
        const el = document.getElementById(lastId);
        if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        sessionStorage.removeItem('scrollTo');
    }
});

/** Global reference to prevent canvas memory leaks and overlapping charts during client-side re-renders. */
let chartInstance = null;

/**
 * Dynamically re-renders the longitudinal chart to isolate specific clinical trends.
 * Allows staff to filter out noise and focus purely on deteriorating or improving domains.
 */
function updateChart(filterType) {
    const rawTypes = rawPrevious.map((prev, i) => {
        const curr = rawCurrent[i];
        if (curr < prev) return "Förbättring";
        if (curr > prev) return "Försämring";
        return "Oförändrad";
    });

    const filteredIndexes = rawTypes
        .map((type, i) => {
            if (filterType === 'improvement' && type === "Förbättring") return i;
            if (filterType === 'deterioration' && type === "Försämring") return i;
            if (filterType === 'all') return i;
            return -1;
        })
        .filter(i => i !== -1);

    const labels = filteredIndexes.map(i => rawLabels[i]);
    const previous = filteredIndexes.map(i => rawPrevious[i]);
    const current = filteredIndexes.map(i => rawCurrent[i]);
    const barColors = filteredIndexes.map(i =>
        rawCurrent[i] < rawPrevious[i] ? "#28a745" : "#dc3545"
    );

    const chartTitle = document.getElementById("chartFilterLabel");
    if (chartTitle) {
        chartTitle.innerText =
            filterType === "all"
                ? "Visar alla resultat"
                : filterType === "improvement"
                    ? "Visar endast förbättringar"
                    : "Visar endast försämringar";
    }

    if (chartInstance) chartInstance.destroy();

    const ctx = document.getElementById('improvementChart');
    if (!ctx) return;

    chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Tidigare',
                    data: previous,
                    backgroundColor: '#ffc107'
                },
                {
                    label: 'Nuvarande',
                    data: current,
                    backgroundColor: barColors
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 4,
                    ticks: { stepSize: 1 },
                    title: {
                        display: true,
                        text: 'Skattning (lägre = bättre)'
                    }
                }
            }
        }
    });
}

/**
 * Orchestrates client-side filtering across both charts, data tables, and feedback summary panels 
 * to provide a synchronized, context-aware dashboard view without server roundtrips.
 */
function filterData(type) {
    updateChart(type);
    filterTable(type);

    const showImprovement = (type === 'all' || type === 'improvement');
    const showDeterioration = (type === 'all' || type === 'deterioration');

    const toggleDisplay = (id, show) => {
        const el = document.getElementById(id);
        if (el) el.style.display = show ? 'block' : 'none';
    };

    toggleDisplay("feedbackImprovement", showImprovement);
    toggleDisplay("feedbackImprovementNote", showImprovement);
    toggleDisplay("feedbackDeterioration", showDeterioration);
    toggleDisplay("feedbackDeteriorationNote", showDeterioration);
}

/**
 * Applies visual filtering to table rows matching the selected clinical trend.
 */
function filterTable(type) {
    const improvementRows = document.querySelectorAll('.table-success');
    const deteriorationRows = document.querySelectorAll('.table-danger');

    improvementRows.forEach(row =>
        row.style.display = type === 'deterioration' ? 'none' : 'table-row'
    );
    deteriorationRows.forEach(row =>
        row.style.display = type === 'improvement' ? 'none' : 'table-row'
    );
}

/**
 * Toggles detailed itemized view for staff during longitudinal comparisons.
 */
function toggleDetailsStaff() {
    const section = document.getElementById('detailsSection');
    const button = document.getElementById('toggleButton');
    const visible = section && section.style.display === 'block';

    if (section) section.style.display = visible ? 'none' : 'block';
    if (button) button.textContent = visible ? '🔍 Visa detaljer' : '🔍 Dölj detaljer';
}

document.addEventListener('DOMContentLoaded', function () {
    const chartEl = document.getElementById('improvementChart');
    if (chartEl) {
        const toggleBtn = document.getElementById('toggleButton');
        const detailSection = document.getElementById('detailsSection');

        if (toggleBtn && detailSection) {
            detailSection.style.display = 'none';
            toggleBtn.textContent = '🔍 Visa detaljer';
        }

        updateChart('all');
        filterTable('all');
    }
});

/**
 * Visualizes the cognitive dissonance or discrepancy between patient self-perception 
 * and clinical observation, central to the DLDA methodology.
 */
function renderStaffComparisonPie(labels, values) {
    const ctx = document.getElementById('comparisonPie');
    if (!ctx) return;

    Chart.register(ChartDataLabels);

    const filtered = labels
        .map((label, index) => ({ label, value: values[index] }))
        .filter(entry => !entry.label.includes("Obesvarad") && entry.value > 0);

    const filteredLabels = filtered.map(entry => entry.label);
    const filteredValues = filtered.map(entry => entry.value);

    const filteredColors = ['#dc3545', '#ffc107', '#198754']; 

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: filteredLabels,
            datasets: [{
                data: filteredValues,
                backgroundColor: filteredColors
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}:${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: {
                        weight: 'bold',
                        size: 14
                    },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

/**
 * Toggles the full itemized comparison table.
 */
function toggleComparisonTable() {
    const section = document.getElementById("questionTableSection");
    if (!section) return;
    section.style.display = section.style.display === "none" ? "block" : "none";
}

/**
 * Dynamically filters the comparison table to isolate critical discrepancy types 
 * (e.g., strong differences or manually flagged items) during active patient consultations.
 */
function applyComparisonFilters() {
    const filter = document.getElementById("filterSelector")?.value;
    const rows = document.querySelectorAll("#comparisonTable tbody tr");

    rows.forEach(row => {
        const type = row.dataset.type;
        const flagged = row.dataset.flagged === "true";
        const commented = row.dataset.commented === "true";

        let show = false;
        if (filter === "all") show = true;
        else if (filter === "flagged") show = flagged;
        else if (filter === "commented") show = commented;
        else show = type === filter;

        row.style.display = show ? "table-row" : "none";
    });
}

/**
 * Renders an isolated overview of patient self-assessment inputs, establishing 
 * the baseline for clinical review before the clinician introduces their own evaluation.
 */
function renderPatientAnswerSummaryPie(labels, data) {
    const ctx = document.getElementById('patientAnswerPie');
    if (!ctx) return;

    Chart.register(ChartDataLabels);

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: [
                    '#198754', 
                    '#74c37e', 
                    '#ffc107', 
                    '#fd7e14', 
                    '#dc3545', 
                    '#dee2e6'  
                ]
            }]
        },
        options: {
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            const label = ctx.label || '';
                            const value = ctx.parsed || 0;
                            return `${label}:${value} frågor`;
                        }
                    }
                },
                datalabels: {
                    color: '#000',
                    font: { weight: 'bold', size: 14 },
                    formatter: (value, ctx) => {
                        const total = ctx.chart.data.datasets[0].data.reduce((a, b) => a + b, 0);
                        if (total === 0 || value === 0) return '';
                        const percent = Math.round((value / total) * 100);
                        return `${percent}%`;
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

/**
 * Toggles visibility of the detailed self-assessment tabular data.
 */
function togglePatientAnswerTable() {
    const section = document.getElementById("patientAnswerTableSection");
    if (!section) return;
    section.style.display = section.style.display === "none" ? "block" : "none";
}

/**
 * Filters the patient self-assessment table by specific severity thresholds.
 */
function applyPatientAnswerFilter() {
    const filter = document.getElementById("patientAnswerFilter")?.value;
    const rows = document.querySelectorAll("#patientAnswerTable tbody tr");

    rows.forEach(row => {
        const svar = row.dataset.svar;
        let show = false;

        if (filter === "all") show = true;
        else if (filter === "null") show = svar === "null";
        else show = svar === filter;

        row.style.display = show ? "table-row" : "none";
    });
}

/**
 * Client-side chronological validation for comparison queries.
 * Ensures the baseline assessment temporally precedes the current assessment 
 * to prevent inverted longitudinal analysis.
 */
function validateDates() {
    const first = document.getElementById("firstSelect");
    const second = document.getElementById("secondSelect");

    const firstDate = new Date(first.options[first.selectedIndex].getAttribute("data-date"));
    const secondDate = new Date(second.options[second.selectedIndex].getAttribute("data-date"));

    const errorDiv = document.getElementById("dateError");

    if (firstDate >= secondDate) {
        errorDiv.style.display = "block";
        return false;
    }

    errorDiv.style.display = "none";
    return true;
}

/**
 * Client-side chronological validation for comparison queries (Staff contextual wrapper).
 */
function validateDates() {
    const first = document.getElementById("firstSelect");
    const second = document.getElementById("secondSelect");

    const firstDate = new Date(first.options[first.selectedIndex].getAttribute("data-date"));
    const secondDate = new Date(second.options[second.selectedIndex].getAttribute("data-date"));

    const errorDiv = document.getElementById("dateError");

    if (firstDate >= secondDate) {
        errorDiv.style.display = "block";
        return false;
    }

    errorDiv.style.display = "none";
    return true;
}

/**
 * Resets the active patient filter state and reloads the base staff assessment list.
 */
function clearSearch() {
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        searchInput.value = '';
    }
    window.location.href = searchInput?.closest("form")?.getAttribute("action") || window.location.href;
}

/**
 * Orchestrates the generation of immutable PDF reports for physical medical records.
 * Uses html2canvas and jsPDF to capture the current DOM state, injects standardized hospital branding (logo/dates),
 * and structures pagination to ensure compliance with clinical documentation standards.
 */
document.addEventListener("DOMContentLoaded", function () {
    const pageId = document.body.id;
    const exportBtn = document.getElementById("downloadPdfBtn");
    if (!exportBtn) return;

    exportBtn.addEventListener("click", async () => {
        const { jsPDF } = window.jspdf;
        let containerSelector = ".container";
        let tableSection = null;
        let filename = "rapport.pdf";

        switch (pageId) {
            case "patient-summary-page":
                tableSection = document.getElementById("patientAnswerTableSection");
                filename = "patient-sammanstallning.pdf";
                break;

            case "comparison-page":
                containerSelector = "#pdf-content";
                tableSection = document.getElementById("questionTableSection");
                filename = "jamforelse-sammanstallning.pdf";
                break;

            case "staff-change-page":
                tableSection = document.getElementById("detailsSection");
                filename = "forandring-over-tid.pdf";
                break;

            case "patient-summary-single":
                filename = "egenbedomning-sammanstallning.pdf";
                break;

            case "patient-change-page":
                tableSection = document.getElementById("detailsSection");
                filename = "forbattring-over-tid.pdf";
                break;
        }

        const container = document.querySelector(containerSelector);
        if (!container) return;

        const wasHidden = tableSection?.style.display === "none";
        if (wasHidden) tableSection.style.display = "block";

        const style = document.createElement("style");
        style.innerHTML = "@media screen {.no-print { display: none !important; }}";
        document.head.appendChild(style);

        try {
            const logo = new Image();
            logo.src = "/images/dlda_logo.png"; 
            await new Promise(resolve => logo.onload = resolve);

            const canvas = await html2canvas(container, { scale: 2 });
            const imgData = canvas.toDataURL("image/png");

            const pdf = new jsPDF("p", "mm", "a4");
            const pdfWidth = pdf.internal.pageSize.getWidth();
            const pdfHeight = pdf.internal.pageSize.getHeight();

            const imgProps = pdf.getImageProperties(imgData);
            const imgWidth = pdfWidth;
            const imgHeight = (imgProps.height * imgWidth) / imgProps.width;

            const logoWidth = 40;
            const logoHeight = (logo.height * logoWidth) / logo.width;
            const offset = 5 + logoHeight + 3;

            let position = 0;
            while (position < imgHeight) {
                pdf.addImage(logo, "PNG", 10, 5, logoWidth, logoHeight);
                pdf.addImage(imgData, "PNG", 0, offset - position, imgWidth, imgHeight);

                const pageNum = pdf.internal.getNumberOfPages();
                const today = new Date().toLocaleDateString();

                pdf.setFontSize(10);
                pdf.text(`Sida ${pageNum}`, pdfWidth - 30, pdfHeight - 10);
                pdf.text(`DLDA – ${today}`, 10, pdfHeight - 10);

                position += pdfHeight;
                if (position < imgHeight) pdf.addPage();
            }

            pdf.save(filename);
        } catch (err) {
            console.error("❌ PDF-fel:", err);
        } finally {
            document.head.removeChild(style);
            if (wasHidden) tableSection.style.display = "none";
        }
    });
});
