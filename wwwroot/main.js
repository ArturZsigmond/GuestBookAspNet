let page = 1;

function loadEntries() {
  const groupBy = document.getElementById("groupBy")?.value || "none";
  const sortBy = document.getElementById("sortBy")?.value || "newest";

  fetch(`/Guestbook/Fetch?page=${page}&groupBy=${groupBy}&sortBy=${sortBy}`)
    .then(response => response.json())
    .then(entries => {
      const container = document.getElementById('entries-container');
      container.innerHTML = '';

      const adminDiv = document.getElementById('admin-flag');
      const isAdmin = adminDiv && adminDiv.dataset.isAdmin === "true";
      console.log("Admin mode:", isAdmin);

      // Grouping: count how many entries per author (for entry count)
      const entryCounts = {};
      for (const entry of entries) {
        entryCounts[entry.authorEmail] = (entryCounts[entry.authorEmail] || 0) + 1;
      }

      for (const entry of entries) {
        const div = document.createElement('div');
        div.className = 'entry';

        const entryCountText = isAdmin
          ? ` (${entryCounts[entry.authorEmail]} entries)`
          : '';

        div.innerHTML = `
          <h3>${entry.title}</h3>
          <p><strong>${entry.authorEmail}</strong>${entryCountText}</p>
          <p>${entry.comment}</p>
          <p>Rating: ${entry.rating} stars</p>
          <small>${new Date(entry.dateCreated).toLocaleString()}</small>
          ${isAdmin ? `
            <div class="admin-controls" style="margin-top: 0.5em;">
              <button onclick="editEntry(${entry.id})">Edit</button>
              <button onclick="deleteEntry(${entry.id})">Delete</button>
            </div>` : ''}
        `;
        container.appendChild(div);
      }
    });
}

function editEntry(id) {
  window.location.href = `/Guestbook/Edit/${id}`;
}

function deleteEntry(id) {
  if (confirm("Are you sure you want to delete this entry?")) {
    fetch(`/Guestbook/Delete/${id}`, { method: "POST" })
      .then(() => loadEntries());
  }
}

const form = document.getElementById('guestbook-form');
if (form) {
  form.addEventListener('submit', function (e) {
    e.preventDefault();

    const title = this.title.value.trim();
    const comment = this.comment.value.trim();
    const rating = this.rating.value;

    if (title.length < 3) {
      alert("Title must be at least 3 characters long.");
      return;
    }

    if (comment.length < 5) {
      alert("Comment must be at least 5 characters long.");
      return;
    }

    const formData = new URLSearchParams();
    formData.append("title", title);
    formData.append("comment", comment);
    formData.append("rating", rating);

    fetch('/Guestbook/Add', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: formData
    }).then(res => {
      if (!res.ok) {
        return res.text().then(msg => alert("Server error: " + msg));
      }
      this.reset();
      loadEntries();
    });
  });
}

document.getElementById('prev-page').onclick = function () {
  if (page > 1) page--;
  loadEntries();
};

document.getElementById('next-page').onclick = function () {
  page++;
  loadEntries();
};

// ✅ Hook into sorting/grouping changes
document.getElementById("groupBy")?.addEventListener("change", () => {
  page = 1;
  loadEntries();
});

document.getElementById("sortBy")?.addEventListener("change", () => {
  page = 1;
  loadEntries();
});

window.onload = loadEntries;
