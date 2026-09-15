const healthEl = document.getElementById('health');
const docListEl = document.getElementById('docList');
const fileInput = document.getElementById('fileInput');
const uploadBtn = document.getElementById('uploadBtn');
const seedBtn = document.getElementById('seedBtn');
const messagesEl = document.getElementById('messages');
const chatForm = document.getElementById('chatForm');
const chatInput = document.getElementById('chatInput');

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

async function refreshHealth() {
  try {
    const res = await fetch('/api/health');
    const data = await res.json();
    if (data.ollamaReachable) {
      healthEl.textContent = `Ollama OK (chat: ${data.chatModel}, embeddings: ${data.embeddingModel})`;
      healthEl.className = 'health ok';
    } else {
      healthEl.textContent = data.detail || 'Ollama not reachable.';
      healthEl.className = 'health bad';
    }
  } catch {
    healthEl.textContent = 'Could not reach the API.';
    healthEl.className = 'health bad';
  }
}

async function refreshDocs() {
  const res = await fetch('/api/documents');
  const docs = await res.json();
  docListEl.innerHTML = '';
  for (const doc of docs) {
    const li = document.createElement('li');
    li.innerHTML = `<span title="${escapeHtml(doc.fileName)}">${escapeHtml(doc.fileName)} (${doc.chunkCount})</span>`;
    const delBtn = document.createElement('button');
    delBtn.textContent = '✕';
    delBtn.className = 'del';
    delBtn.onclick = async () => {
      await fetch(`/api/documents/${doc.id}`, { method: 'DELETE' });
      refreshDocs();
    };
    li.appendChild(delBtn);
    docListEl.appendChild(li);
  }
}

function addMessage(role, text, citations) {
  const div = document.createElement('div');
  div.className = `msg ${role}`;
  div.innerText = text;

  if (citations && citations.length > 0) {
    const cites = document.createElement('div');
    cites.className = 'citations';
    cites.innerHTML = citations
      .map(c => `<div>[${c.index}] ${escapeHtml(c.documentName)} (chunk ${c.chunkIndex}, score ${c.score.toFixed(3)}) &mdash; ${escapeHtml(c.excerpt)}</div>`)
      .join('');
    div.appendChild(cites);
  }

  messagesEl.appendChild(div);
  messagesEl.scrollTop = messagesEl.scrollHeight;
}

uploadBtn.addEventListener('click', async () => {
  const file = fileInput.files[0];
  if (!file) {
    alert('Choose a file first.');
    return;
  }

  uploadBtn.disabled = true;
  uploadBtn.textContent = 'Uploading…';
  try {
    const formData = new FormData();
    formData.append('file', file);
    const res = await fetch('/api/documents', { method: 'POST', body: formData });
    if (!res.ok) {
      alert(`Upload failed: ${await res.text()}`);
      return;
    }
    fileInput.value = '';
    await refreshDocs();
  } finally {
    uploadBtn.disabled = false;
    uploadBtn.textContent = 'Upload document';
  }
});

seedBtn.addEventListener('click', async () => {
  seedBtn.disabled = true;
  seedBtn.textContent = 'Loading…';
  try {
    const res = await fetch('/api/documents/seed-samples', { method: 'POST' });
    if (!res.ok) {
      alert(`Failed to load samples: ${await res.text()}`);
      return;
    }
    await refreshDocs();
  } finally {
    seedBtn.disabled = false;
    seedBtn.textContent = 'Load sample docs';
  }
});

chatForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const message = chatInput.value.trim();
  if (!message) return;

  addMessage('user', message);
  chatInput.value = '';

  const res = await fetch('/api/chat', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ message }),
  });

  if (!res.ok) {
    addMessage('assistant', `Error: ${await res.text()}`);
    return;
  }

  const data = await res.json();
  addMessage('assistant', data.answer, data.citations);
});

refreshHealth();
refreshDocs();
setInterval(refreshHealth, 15000);
