(() => {
    const sectionIds = [
        "registration-summary",
        "registration-details",
        "registration-table",
        "financial-summary",
        "course-finance",
        "course-performance",
        "instructor-performance",
        "learning-progress"
    ];

    const icons = {
        excel: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M13 3h6a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-6z" fill="currentColor" opacity=".18"/><path d="M13 3h6a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-6M13 3v18M3 6l10-2v16L3 18z" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round"/><path d="m5.5 9 4 6m0-6-4 6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg>',
        pdf: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 2h8l5 5v15H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2z" fill="currentColor" opacity=".12"/><path d="M14 2v5h5M6 2h8l5 5v15H6a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2z" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round"/><path d="M7 16h2a1.5 1.5 0 0 0 0-3H7v5m6 0v-5h1a2.5 2.5 0 0 1 0 5zm5-5h-2v5m0-2h2" fill="none" stroke="currentColor" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"/></svg>'
    };

    function addStyles() {
        if (document.getElementById("report-export-styles")) return;
        const style = document.createElement("style");
        style.id = "report-export-styles";
        style.textContent = `
            .report-section .section-title,
            section.registration-breakdown .section-title {
                display:flex;
                align-items:center;
                flex-wrap:wrap;
                gap:12px;
            }
            .report-export-actions {
                display:inline-flex;
                align-items:center;
                gap:7px;
                margin-inline-start:auto;
                flex-shrink:0;
            }
            .report-export-btn {
                display:inline-flex;
                align-items:center;
                justify-content:center;
                gap:6px;
                min-height:34px;
                padding:6px 11px;
                border:1px solid;
                border-radius:8px;
                background:#fff;
                font:inherit;
                font-size:12px;
                font-weight:600;
                line-height:1.2;
                cursor:pointer;
                transition:background .15s ease,transform .15s ease;
                white-space:nowrap;
            }
            .report-export-btn svg {
                width:18px;
                height:18px;
                flex-shrink:0;
            }
            .report-export-excel {
                color:#217346;
                border-color:#b8d9c5;
                background:#f3faf5;
            }
            .report-export-excel:hover { background:#e2f3e7; }
            .report-export-pdf {
                color:#b42318;
                border-color:#f0c4c0;
                background:#fff6f5;
            }
            .report-export-pdf:hover { background:#fde9e7; }
            .report-export-btn:active { transform:scale(.97); }
            @media(max-width:600px) {
                .report-export-actions { margin-inline-start:0; }
                .report-export-btn { padding:6px 8px; }
            }
            @media print { .report-export-actions { display:none!important; } }
        `;
        document.head.appendChild(style);
    }

    function exportSection(section, format) {
        const copy = section.cloneNode(true);
        copy.querySelectorAll(".report-export-actions").forEach(el => el.remove());

        const title = section.querySelector(".section-title h2,.section-title h3,h2,h3,h4")?.innerText || section.id;
        const styles = `
            body { font-family:Arial,sans-serif; direction:rtl; padding:20px; color:#263d4a; }
            table { width:100%; border-collapse:collapse; margin:12px 0; }
            th,td { border:1px solid #bbb; padding:7px; text-align:right; }
            th { background:#eee; }
            h2,h3,h4 { margin-bottom:14px; }
        `;

        if (format === "excel") {
            const html = `<!doctype html><html><head><meta charset="utf-8"><style>${styles}</style></head><body>${copy.outerHTML}</body></html>`;
            const blob = new Blob(["\ufeff", html], { type:"application/vnd.ms-excel;charset=utf-8" });
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = `${section.id}.xls`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            setTimeout(() => URL.revokeObjectURL(url), 1000);
            return;
        }

        const win = window.open("", "_blank");
        if (!win) {
            alert("پنجره خروجی مسدود شده است؛ اجازه بازشدن پنجره را بدهید.");
            return;
        }
        win.document.open();
        win.document.write(`<!doctype html><html><head><meta charset="utf-8"><title>${title}</title><style>${styles}@media print{body{padding:0}}</style></head><body>${copy.outerHTML}<script>window.onload=()=>setTimeout(()=>window.print(),300);<\/script></body></html>`);
        win.document.close();
    }

    function addExportButtons() {
        sectionIds.forEach(id => {
            const section = document.getElementById(id);
            if (!section) return;

            const header = section.querySelector(":scope > .section-title");
            if (!header || header.querySelector(":scope > .report-export-actions")) return;

            const actions = document.createElement("div");
            actions.className = "report-export-actions";

            [
                { label: "اکسل", format: "excel", className: "report-export-excel" },
                { label: "PDF", format: "pdf", className: "report-export-pdf" }
            ].forEach(item => {
                const button = document.createElement("button");
                button.type = "button";
                button.className = `report-export-btn ${item.className}`;
                button.innerHTML = `${icons[item.format]}<span>${item.label}</span>`;
                button.title = `خروجی ${item.label}`;
                button.setAttribute("aria-label", `خروجی ${item.label}`);
                button.addEventListener("click", () => exportSection(section, item.format));
                actions.appendChild(button);
            });

            header.appendChild(actions);
        });
    }

    function start() {
        addStyles();
        addExportButtons();

        const observer = new MutationObserver(() => {
            addExportButtons();
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start, { once: true });
    } else {
        start();
    }
})();
