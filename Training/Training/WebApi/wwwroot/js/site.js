/**
 * AI Comment Moderation — site.js
 * Kết nối thật với .NET WebAPI /api/predict
 */

// ─── App State ───────────────────────────────────────────────────────────────
let state = {
    inputText: '',
    isAnalyzing: false,
    result: null,       // { status, confidence, text, recommendedAction }
    history: [],        // Không dùng mock data nữa — chỉ lưu kết quả thật
    feedbackGiven: false
};

// ─── DOM References ───────────────────────────────────────────────────────────
const $ = id => document.getElementById(id);

// ─── SVG Icons ────────────────────────────────────────────────────────────────
const SVG = {
    shield: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>`,
    shieldGray: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>`,
    alert: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3"/><path d="M12 9v4"/><path d="M12 17h.01"/></svg>`,
    clock: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>`,
    thumbsUp: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 10v12"/><path d="M15 5.88 14 10h5.83a2 2 0 0 1 1.92 2.56l-2.33 8A2 2 0 0 1 17.5 22H4a2 2 0 0 1-2-2v-8a2 2 0 0 1 2-2h2.76a2 2 0 0 0 1.79-1.11L12 2h0a3.13 3.13 0 0 1 3 3.88Z"/></svg>`,
    thumbsDown: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 14V2"/><path d="M9 18.12 10 14H4.17a2 2 0 0 1-1.92-2.56l2.33-8A2 2 0 0 1 6.5 2H20a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2h-2.76a2 2 0 0 0-1.79 1.11L12 22h0a3.13 3.13 0 0 1-3-3.88Z"/></svg>`,
    errorIcon: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`
};

// ─── Circular Progress SVG Builder ───────────────────────────────────────────
function buildCircularProgress(value, color, size = 85, strokeWidth = 7) {
    const radius = (size - strokeWidth) / 2;
    const circumference = radius * 2 * Math.PI;
    const offset = circumference - (value / 100) * circumference;

    return `
    <div class="circular-progress-wrap">
      <svg width="${size}" height="${size}">
        <circle cx="${size / 2}" cy="${size / 2}" r="${radius}"
          stroke="#e5e7eb" stroke-width="${strokeWidth}" fill="none"/>
        <circle cx="${size / 2}" cy="${size / 2}" r="${radius}"
          stroke="${color}" stroke-width="${strokeWidth}" fill="none"
          stroke-dasharray="${circumference}"
          stroke-dashoffset="${offset}"
          stroke-linecap="round"
          style="transition: stroke-dashoffset 0.7s ease-out; filter: drop-shadow(0 2px 4px rgba(0,0,0,0.1));"/>
      </svg>
      <div class="circular-progress-value">
        <span style="color: ${color}">${value}%</span>
      </div>
    </div>`;
}

// ─── History Item HTML Builder ────────────────────────────────────────────────
function buildHistoryItem(entry) {
    return `
    <button class="history-item" data-id="${entry.id}" onclick="loadFromHistory('${entry.id}')">
      <div class="history-item-inner">
        <div class="history-dot ${entry.status}"></div>
        <div class="history-content">
          <p class="history-text">${entry.text.replace(/</g, '&lt;')}</p>
          <span class="history-confidence">${entry.confidence}%</span>
        </div>
      </div>
    </button>`;
}

// ─── Render: History List ─────────────────────────────────────────────────────
function renderHistory() {
    const list = $('history-list');
    if (!list) return;

    if (state.history.length === 0) {
        list.innerHTML = `<div class="history-empty">No analyses yet</div>`;
        return;
    }

    list.innerHTML = state.history.map(buildHistoryItem).join('');
}

// ─── Render: Result Panel ─────────────────────────────────────────────────────
function renderResult() {
    const panel = $('result-panel');
    const feedbackSection = $('feedback-section');
    if (!panel) return;

    if (state.isAnalyzing) {
        panel.innerHTML = `
      <div class="result-analyzing">
        <div class="analyzing-inner">
          <div class="spinner-wrap">
            <div class="pulse-ring"></div>
            <div class="ring-outer-bg"></div>
            <div class="ring-outer-spin"></div>
            <div class="ring-mid-bg"></div>
            <div class="ring-mid-spin"></div>
            <div class="spinner-core">
              <div class="spinner-core-inner"></div>
            </div>
          </div>
          <h3 class="analyzing-title">Analyzing Content</h3>
          <p class="analyzing-desc">AI is processing language patterns and context</p>
          <div class="analyzing-steps">
            <div class="step-item"><div class="step-dot"></div><span>Tokenizing</span></div>
            <div class="step-sep"></div>
            <div class="step-item"><div class="step-dot"></div><span>Analyzing</span></div>
            <div class="step-sep"></div>
            <div class="step-item"><div class="step-dot"></div><span>Scoring</span></div>
          </div>
        </div>
      </div>`;
        if (feedbackSection) feedbackSection.style.display = 'none';
        return;
    }

    if (!state.result) {
        panel.innerHTML = `
      <div class="result-empty">
        <div class="empty-icon-wrap">${SVG.shieldGray}</div>
        <h2 class="empty-title">Ready to Analyze</h2>
        <p class="empty-desc">Enter a comment in the text area and click analyze to get started</p>
      </div>`;
        if (feedbackSection) feedbackSection.style.display = 'none';
        return;
    }

    // ── ERROR STATE ───────────────────────────────────────────────────────────
    if (state.result.isError) {
        panel.innerHTML = `
      <div class="result-panel fade-in" style="border-top: 4px solid #f59e0b;">
        <div class="status-block">
          <div class="status-header">
            <div class="status-icon-toxic" style="background: #fef3c7; color: #d97706;">${SVG.errorIcon}</div>
            <div>
              <h2 style="color: #d97706; font-size: 1.25rem; font-weight: 700; margin: 0;">Connection Error</h2>
              <p class="status-desc">${state.result.errorMessage}</p>
            </div>
          </div>
        </div>
      </div>`;
        if (feedbackSection) feedbackSection.style.display = 'none';
        return;
    }

    // ── RESULT STATE ──────────────────────────────────────────────────────────
    const r = state.result;
    const isSafe = r.status === 'safe';
    const color = isSafe ? '#22c55e' : '#ef4444';
    const statusClass = isSafe ? 'safe' : 'toxic';
    const iconHtml = isSafe
        ? `<div class="status-icon-safe">${SVG.shield}</div>`
        : `<div class="status-icon-toxic">${SVG.alert}</div>`;
    const titleClass = isSafe ? 'status-title-safe' : 'status-title-toxic';
    const titleText = isSafe ? 'Safe Content' : 'Toxic Content';
    const descText = isSafe
        ? 'This comment is respectful and appropriate'
        : 'This comment may contain harmful language';

    panel.innerHTML = `
    <div class="result-panel ${statusClass} fade-in">
      <div class="status-block">
        <div class="status-header">
          ${iconHtml}
          <div>
            <h2 class="${titleClass}">${titleText}</h2>
            <p class="status-desc">${descText}</p>
          </div>
        </div>
      </div>

      <div class="confidence-block">
        <div class="confidence-inner">
          <div>
            <div class="section-label">Confidence</div>
            ${buildCircularProgress(r.confidence, color)}
          </div>
          <p class="confidence-text">
            The AI is <strong>${r.confidence}% confident</strong> in this assessment
          </p>
        </div>
      </div>

      <div class="analyzed-block">
        <div class="section-label">Analyzed Text</div>
        <div class="analyzed-text-box">
          <p>${r.text.replace(/</g, '&lt;')}</p>
        </div>
      </div>

      <div class="analyzed-block" style="margin-top: 0.75rem;">
        <div class="section-label">Recommended Action</div>
        <div class="analyzed-text-box" style="padding: 0.5rem 0.75rem;">
          <p style="font-weight: 600; color: ${isSafe ? '#16a34a' : '#dc2626'};">${r.recommendedAction}</p>
        </div>
      </div>
    </div>`;

    // Animate circular progress
    requestAnimationFrame(() => {
        const radius = (85 - 7) / 2;
        const circumference = radius * 2 * Math.PI;
        const targetOffset = circumference - (r.confidence / 100) * circumference;
        const progressCircle = panel.querySelectorAll('circle')[1];
        if (progressCircle) {
            progressCircle.style.strokeDashoffset = circumference;
            requestAnimationFrame(() => {
                progressCircle.style.strokeDashoffset = targetOffset;
            });
        }
    });

    if (feedbackSection) {
        feedbackSection.style.display = 'block';
        state.feedbackGiven = false;
        renderFeedback();
    }
}

// ─── Render: Feedback ─────────────────────────────────────────────────────────
function renderFeedback() {
    const fb = $('feedback-buttons');
    if (!fb) return;

    if (state.feedbackGiven) {
        fb.innerHTML = `<div class="feedback-thanks">Thanks for your feedback</div>`;
        return;
    }

    fb.innerHTML = `
    <div class="feedback-row">
      <span class="feedback-label">Was this accurate?</span>
      <button class="btn-feedback correct" onclick="handleFeedback(true)">${SVG.thumbsUp}<span>Correct</span></button>
      <button class="btn-feedback wrong" onclick="handleFeedback(false)">${SVG.thumbsDown}<span>Wrong</span></button>
    </div>`;
}

// ─── Analyze Comment — Gọi thật tới /api/predict ─────────────────────────────
const MIN_LOADING_MS = 2200; // Thời gian hiển thị loading tối thiểu (ms)

async function analyzeComment() {
    const text = state.inputText.trim();
    if (!text || state.isAnalyzing) return;

    state.isAnalyzing = true;
    state.result = null;
    updateAnalyzeButton();
    renderResult();

    try {
       
        const API_URL = 'https://bafflingly-unejective-gilberte.ngrok-free.dev/api/predict';
        const [response] = await Promise.all([
            fetch(API_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ TextContent: text })
            }),
            new Promise(resolve => setTimeout(resolve, MIN_LOADING_MS))
        ]);

        if (!response.ok) {
            // Lấy message lỗi từ backend nếu có
            let errMsg = `Server error: ${response.status} ${response.statusText}`;
            try {
                const errBody = await response.json();
                if (errBody?.error) errMsg = errBody.error;
            } catch (_) { /* ignore parse error */ }
            throw new Error(errMsg);
        }

        const data = await response.json();

        /**
         * Backend trả về:
         * {
         *   message: "...",          ← text gốc
         *   isToxic: true/false,
         *   confidenceScore: "92.5%",
         *   recommendedAction: "Block & Review" | "Allow"
         * }
         */
        const confidence = Math.round(parseFloat(data.confidenceScore));

        const newResult = {
            status: data.isToxic ? 'toxic' : 'safe',
            confidence,
            text: data.message,
            recommendedAction: data.recommendedAction
        };

        state.result = newResult;
        state.isAnalyzing = false;

        // Thêm vào history (tối đa 10 mục)
        const newEntry = {
            ...newResult,
            id: Date.now().toString(),
            timestamp: new Date()
        };
        state.history = [newEntry, ...state.history].slice(0, 10);

    } catch (err) {
        console.error('API error:', err);
        state.result = {
            isError: true,
            errorMessage: err.message || 'Could not reach the backend. Is the server running?'
        };
        state.isAnalyzing = false;
    }

    updateAnalyzeButton();
    renderHistory();
    renderResult();
}

// ─── Load from History ────────────────────────────────────────────────────────
function loadFromHistory(id) {
    const entry = state.history.find(h => h.id === id);
    if (!entry) return;

    state.inputText = entry.text;
    state.result = {
        status: entry.status,
        confidence: entry.confidence,
        text: entry.text,
        recommendedAction: entry.recommendedAction
    };

    const textarea = $('comment-input');
    if (textarea) textarea.value = entry.text;

    updateAnalyzeButton();
    renderResult();
}

// ─── Handle Feedback ──────────────────────────────────────────────────────────
function handleFeedback(isCorrect) {
    console.log('Feedback received:', isCorrect);
    state.feedbackGiven = true;
    renderFeedback();
}

// ─── Update Analyze Button ────────────────────────────────────────────────────
function updateAnalyzeButton() {
    const btn = $('btn-analyze');
    if (!btn) return;

    if (state.isAnalyzing) {
        btn.disabled = true;
        btn.innerHTML = `
      <svg class="spin" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
      </svg>
      Analyzing...`;
    } else {
        btn.disabled = !state.inputText.trim();
        btn.innerHTML = 'Analyze Comment';
    }
}

// ─── Init ─────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const textarea = $('comment-input');
    const btnAnalyze = $('btn-analyze');

    if (textarea) {
        textarea.addEventListener('input', e => {
            state.inputText = e.target.value;
            updateAnalyzeButton();
        });

        textarea.addEventListener('keydown', e => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
                analyzeComment();
            }
        });
    }

    if (btnAnalyze) {
        btnAnalyze.addEventListener('click', analyzeComment);
    }

    renderHistory();
    renderResult();
    updateAnalyzeButton();
});

// Expose globals (dùng trong onclick attribute)
window.loadFromHistory = loadFromHistory;
window.handleFeedback = handleFeedback;