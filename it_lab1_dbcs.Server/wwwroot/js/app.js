let currentUser = null;
let currentDatabase = null;
let currentTable = null;
let selectedRowIndex = -1;
let isModified = false;
let tempSchemaFields = [];
let isEditingRow = false;

let createTableModal, rowModal, inputModal;

document.addEventListener('DOMContentLoaded', () => {
    currentUser = localStorage.getItem('currentUser');
    if (!currentUser) {
        window.location.href = 'login.html';
        return;
    }

    document.getElementById('lblCurrentUser').innerText = currentUser;

    createTableModal = new bootstrap.Modal(document.getElementById('createTableModal'));
    rowModal = new bootstrap.Modal(document.getElementById('rowModal'));
    inputModal = new bootstrap.Modal(document.getElementById('inputModal'));

    loadUserDatabases();
});

function markAsModified(modified = true) {
    isModified = modified;
    const select = document.getElementById('cmbDatabases');
    if (select && select.selectedOptions.length > 0 && currentDatabase) {
        const opt = select.selectedOptions[0];
        const dbName = currentDatabase.Name || currentDatabase.name || 'База даних';
        opt.innerText = isModified ? `${dbName} *` : dbName;
    }
}


async function loadUserDatabases() {
    try {
        const res = await fetch(`/api/database/user/${currentUser}`);
        const dbNames = await res.json();

        const select = document.getElementById('cmbDatabases');
        select.innerHTML = '';

        if (dbNames && dbNames.length > 0) {
            dbNames.forEach(name => {
                const opt = document.createElement('option');
                opt.value = name;
                opt.innerText = name;
                select.appendChild(opt);
            });

            await loadDatabase(dbNames[0]);
        } else {
            // Якщо для користувача немає баз, створюємо першу базову
            const defaultDbName = `DefaultDB_${currentUser}`;
            createNewDatabaseObj(defaultDbName);

            const opt = document.createElement('option');
            opt.value = defaultDbName;
            opt.innerText = defaultDbName;
            select.appendChild(opt);
        }
    } catch (err) {
        console.error('Помилка завантаження баз даних:', err);
        const defaultDbName = `DefaultDB_${currentUser}`;
        createNewDatabaseObj(defaultDbName);
    }
}

async function onDatabaseSelectChange() {
    if (isModified && currentDatabase) {
        if (!confirm(`База даних '${currentDatabase.Name}' має незбережені зміни. Переключитися без збереження?`)) {
            document.getElementById('cmbDatabases').value = currentDatabase.Name;
            return;
        }
    }

    const selectedName = document.getElementById('cmbDatabases').value;
    if (selectedName) {
        await loadDatabase(selectedName);
    }
}

async function loadDatabase(name) {
    try {
        const res = await fetch(`/api/database/${name}`);
        if (res.ok) {
            const db = await res.json();

            currentDatabase = {
                Name: db.Name || db.name || name,
                OwnerUsername: db.OwnerUsername || db.ownerUsername || currentUser,
                Tables: db.Tables || db.tables || []
            };

            currentDatabase.Tables.forEach(t => {
                t.Name = t.Name || t.name;
                t.Schema = t.Schema || t.schema || { Fields: [] };
                t.Schema.Fields = t.Schema.Fields || t.Schema.fields || [];

                t.Schema.Fields.forEach(f => {
                    f.Name = f.Name || f.name;
                    f.Type = f.Type || f.type;
                    f.AllowedEnumValues = f.AllowedEnumValues || f.allowedEnumValues || [];
                });

                t.Rows = t.Rows || t.rows || [];
                t.Rows.forEach(r => {
                    r.Values = r.Values || r.values || {};
                });
            });

            currentTable = null;
            selectedRowIndex = -1;
            markAsModified(false);
            renderDatabaseUI();
        } else {
            alert('Не вдалося завантажити базу даних з сервера');
        }
    } catch (err) {
        console.error(err);
        alert('Помилка з’єднання з сервером при завантаженні БД');
    }
}

function promptCreateNewDb() {
    if (isModified && currentDatabase) {
        if (!confirm('Поточна база даних має незбережені зміни. Продовжити створення нової?')) {
            return;
        }
    }

    showInputDialog('Створення нової БД', 'Введіть назву для нової бази даних:', `DB_${Date.now().toString().slice(-4)}`, (newDbName) => {
        createNewDatabaseObj(newDbName);
        const select = document.getElementById('cmbDatabases');
        const opt = document.createElement('option');
        opt.value = newDbName;
        opt.innerText = `${newDbName} *`;
        opt.selected = true;
        select.appendChild(opt);

        markAsModified(true);
        renderDatabaseUI();
    });
}

function createNewDatabaseObj(name) {
    currentDatabase = {
        Name: name,
        OwnerUsername: currentUser,
        Tables: []
    };
    currentTable = null;
    selectedRowIndex = -1;
    markAsModified(false);
}

async function saveDatabaseOnServer() {
    if (!currentDatabase) return;

    try {
        const res = await fetch('/api/database/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(currentDatabase)
        });

        const data = await res.json();
        if (res.ok) {
            markAsModified(false);
            alert(`Базу даних '${currentDatabase.Name}' успішно збережено у папку Databases/!`);
        } else {
            alert(data.message || 'Помилка збереження');
        }
    } catch (err) {
        alert('Не вдалося з’єднатися з сервером');
    }
}

function renderDatabaseUI() {
    const lst = document.getElementById('lstTables');
    lst.innerHTML = '';

    if (!currentDatabase || !currentDatabase.Tables) return;

    currentDatabase.Tables.forEach((t) => {
        const btn = document.createElement('button');
        btn.className = `list-group-item list-group-item-action ${currentTable && currentTable.Name === t.Name ? 'active' : ''}`;
        btn.innerText = t.Name;
        btn.onclick = () => selectTable(t);
        lst.appendChild(btn);
    });

    if (currentDatabase.Tables.length > 0 && !currentTable) {
        selectTable(currentDatabase.Tables[0]);
    } else if (currentDatabase.Tables.length === 0) {
        currentTable = null;
        renderTableUI();
    }
}

function selectTable(table) {
    currentTable = table;
    selectedRowIndex = -1;
    renderDatabaseUI();
    renderTableUI();
}

function renderTableUI() {
    const header = document.getElementById('dgHeader');
    const body = document.getElementById('dgBody');
    header.innerHTML = '';
    body.innerHTML = '';

    if (!currentTable) {
        document.getElementById('lblTableName').innerText = 'Таблиця не обрана';
        return;
    }

    document.getElementById('lblTableName').innerText = `Таблиця: ${currentTable.Name}`;

    // Заголовки з підтримкою сортування
    const trHeader = document.createElement('tr');
    currentTable.Schema.Fields.forEach(f => {
        const th = document.createElement('th');
        th.innerText = `${f.Name} (${f.Type}) ↕`;
        th.onclick = () => sortTableByField(f.Name);
        trHeader.appendChild(th);
    });
    header.appendChild(trHeader);

    // Відображення рядків
    currentTable.Rows.forEach((r, idx) => {
        const tr = document.createElement('tr');
        if (idx === selectedRowIndex) tr.classList.add('table-primary');

        tr.onclick = () => {
            selectedRowIndex = idx;
            renderTableUI();
        };

        currentTable.Schema.Fields.forEach(f => {
            const td = document.createElement('td');
            const val = r.Values[f.Name];
            td.innerText = val !== null && val !== undefined ? val : '';
            tr.appendChild(td);
        });

        body.appendChild(tr);
    });
}

function sortTableByField(fieldName) {
    if (!currentTable || currentTable.Rows.length === 0) return;

    let asc = currentTable._lastSortField === fieldName ? !currentTable._lastSortAsc : true;
    currentTable._lastSortField = fieldName;
    currentTable._lastSortAsc = asc;

    const field = currentTable.Schema.Fields.find(f => f.Name === fieldName);

    currentTable.Rows.sort((a, b) => {
        let v1 = a.Values[fieldName] || '';
        let v2 = b.Values[fieldName] || '';

        if (field.Type === 'Integer') {
            return asc ? (parseInt(v1) || 0) - (parseInt(v2) || 0) : (parseInt(v2) || 0) - (parseInt(v1) || 0);
        } else if (field.Type === 'Real') {
            return asc ? (parseFloat(v1) || 0) - (parseFloat(v2) || 0) : (parseFloat(v2) || 0) - (parseFloat(v1) || 0);
        } else {
            return asc ? v1.localeCompare(v2) : v2.localeCompare(v1);
        }
    });

    renderTableUI();
}

// --- КОНСТРУКТОР ТАБЛИЦІ (Modal) ---

function openCreateTableModal() {
    if (!currentDatabase) return alert('Спочатку створіть або оберіть базу даних');
    tempSchemaFields = [];
    document.getElementById('txtTableName').value = '';
    renderSchemaFieldsTable();
    createTableModal.show();
}

function toggleEnumInput() {
    const type = document.getElementById('cmbFieldType').value;
    const input = document.getElementById('txtEnumValues');

    if (type === 'Enum' || type === 'Set') {
        input.disabled = false;
        input.placeholder = type === 'Enum'
            ? 'Варіанти Enum через кому'
            : 'Варіанти Set через кому';
    } else {
        input.disabled = true;
        input.value = '';
        input.placeholder = 'Заблоковано (тільки для Enum / Set)';
    }
}

function addColumnToSchema() {
    const name = document.getElementById('txtFieldName').value.trim();
    const type = document.getElementById('cmbFieldType').value;
    const enumVal = document.getElementById('txtEnumValues').value.trim();

    if (!name) return alert('Введіть назву поля');
    if (tempSchemaFields.some(f => f.Name.toLowerCase() === name.toLowerCase())) {
        return alert('Поле з такою назвою вже додано');
    }

    let allowed = [];
    if (type === 'Enum' || type === 'Set') {
        if (!enumVal) return alert(`Вкажіть дозволені варіанти для типу ${type} через кому`);
        allowed = enumVal.split(',').map(s => s.trim()).filter(s => s.length > 0);
    }

    tempSchemaFields.push({
        Name: name,
        Type: type,
        AllowedEnumValues: allowed
    });

    document.getElementById('txtFieldName').value = '';
    document.getElementById('txtEnumValues').value = '';
    toggleEnumInput();
    renderSchemaFieldsTable();
}

function renderSchemaFieldsTable() {
    const tbody = document.getElementById('tblSchemaFields');
    tbody.innerHTML = '';

    tempSchemaFields.forEach((f, i) => {
        const allowedStr = (f.AllowedEnumValues && f.AllowedEnumValues.length > 0)
            ? f.AllowedEnumValues.join(', ')
            : '—';

        tbody.innerHTML += `
            <tr>
                <td>${f.Name}</td>
                <td>${f.Type}</td>
                <td>${allowedStr}</td>
                <td><button class="btn btn-danger btn-sm" onclick="tempSchemaFields.splice(${i},1);renderSchemaFieldsTable();">X</button></td>
            </tr>
        `;
    });
}
function submitCreateTable() {
    const name = document.getElementById('txtTableName').value.trim();
    if (!name) return alert('Вкажіть назву таблиці');
    if (tempSchemaFields.length === 0) return alert('Таблиця повинна мати хоча б одну колонку');

    const newTable = {
        Name: name,
        Schema: { Fields: tempSchemaFields },
        Rows: []
    };

    currentDatabase.Tables.push(newTable);
    markAsModified(true);
    createTableModal.hide();
    selectTable(newTable);
}


function openAddRowModal() {
    if (!currentTable) return alert('Оберіть таблицю');
    isEditingRow = false;
    buildDynamicRowForm({});
    document.getElementById('rowModalTitle').innerText = 'Новий запис';
    rowModal.show();
}

function openEditRowModal() {
    if (!currentTable || selectedRowIndex < 0) return alert('Оберіть рядок для редагування');
    isEditingRow = true;
    buildDynamicRowForm(currentTable.Rows[selectedRowIndex].Values);
    document.getElementById('rowModalTitle').innerText = 'Редагування запису';
    rowModal.show();
}

function buildDynamicRowForm(existingValues) {
    const form = document.getElementById('dynamicRowForm');
    document.getElementById('rowValidationError').classList.add('d-none');
    form.innerHTML = '';

    currentTable.Schema.Fields.forEach(f => {
        const mb = document.createElement('div');
        mb.className = 'mb-3';

        const lbl = document.createElement('label');
        lbl.className = 'form-label fw-bold';
        lbl.innerText = `${f.Name} (${f.Type})`;
        mb.appendChild(lbl);

        const val = existingValues[f.Name] || '';

        if (f.Type === 'Enum') {
            const select = document.createElement('select');
            select.className = 'form-select';
            select.dataset.fieldName = f.Name;

            f.AllowedEnumValues.forEach(opt => {
                const o = document.createElement('option');
                o.value = opt;
                o.innerText = opt;
                if (opt === val) o.selected = true;
                select.appendChild(o);
            });
            mb.appendChild(select);
        } else if (f.Type === 'Set') {
            const currentSelected = val.split(',').map(s => s.trim());

            const setContainer = document.createElement('div');
            setContainer.className = 'border rounded p-2 bg-white';
            setContainer.dataset.fieldName = f.Name;
            setContainer.dataset.fieldType = 'Set';

            f.AllowedEnumValues.forEach(opt => {
                const checkDiv = document.createElement('div');
                checkDiv.className = 'form-check';

                const check = document.createElement('input');
                check.type = 'checkbox';
                check.className = 'form-check-input set-option-check';
                check.value = opt;
                if (currentSelected.includes(opt)) check.checked = true;

                const checkLbl = document.createElement('label');
                checkLbl.className = 'form-check-label';
                checkLbl.innerText = opt;

                checkDiv.appendChild(check);
                checkDiv.appendChild(checkLbl);
                setContainer.appendChild(checkDiv);
            });
            mb.appendChild(setContainer);
        } else {
            const input = document.createElement('input');
            input.className = 'form-control';
            input.dataset.fieldName = f.Name;
            input.value = val;

            if (f.Type === 'Integer') input.type = 'number';
            else if (f.Type === 'Real') input.type = 'text';
            else if (f.Type === 'Char') input.maxLength = 1;
            else if (f.Type === 'Email') input.type = 'email';
            else input.type = 'text';

            mb.appendChild(input);
        }

        form.appendChild(mb);
    });
}

async function submitRowForm() {
    const inputs = document.querySelectorAll('#dynamicRowForm [data-field-name]');
    const rowValues = {};

    inputs.forEach(inp => {
        const fieldName = inp.dataset.fieldName;

        if (inp.dataset.fieldType === 'Set') {
            // Збираємо відмічені чекбокси у рядок через кому
            const checkedBoxes = inp.querySelectorAll('.set-option-check:checked');
            const selectedValues = Array.from(checkedBoxes).map(c => c.value);
            rowValues[fieldName] = selectedValues.join(', ');
        } else {
            rowValues[fieldName] = inp.value.trim();
        }
    });

    const candidateRow = {
        Id: isEditingRow ? currentTable.Rows[selectedRowIndex].Id : '00000000-0000-0000-0000-000000000000',
        Values: rowValues
    };

    try {
        const res = await fetch('/api/table/validate-row', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Schema: currentTable.Schema, Row: candidateRow })
        });

        const data = await res.json();

        if (!res.ok) {
            const errDiv = document.getElementById('rowValidationError');
            errDiv.innerText = data.message || 'Помилка валідації';
            errDiv.classList.remove('d-none');
            return;
        }

        if (isEditingRow) {
            currentTable.Rows[selectedRowIndex].Values = rowValues;
        } else {
            currentTable.Rows.push(candidateRow);
        }

        markAsModified(true);
        rowModal.hide();
        renderTableUI();
    } catch (err) {
        alert('Помилка валідації на сервері');
    }
}

function deleteSelectedRow() {
    if (!currentTable || selectedRowIndex < 0) return alert('Оберіть рядок');
    if (confirm('Видалити обраний запис?')) {
        currentTable.Rows.splice(selectedRowIndex, 1);
        selectedRowIndex = -1;
        markAsModified(true);
        renderTableUI();
    }
}

function promptRenameDb() {
    if (!currentDatabase) return;
    showInputDialog('Перейменування БД', 'Введіть нову назву:', currentDatabase.Name, (val) => {
        const oldName = currentDatabase.Name;
        currentDatabase.Name = val;
        const select = document.getElementById('cmbDatabases');
        for (let opt of select.options) {
            if (opt.value === oldName) {
                opt.value = val;
                opt.innerText = isModified ? `${val} *` : val;
                break;
            }
        }

        markAsModified(true);
        renderDatabaseUI();
    });
}

function promptRenameTable() {
    if (!currentTable) return;
    showInputDialog('Перейменування таблиці', 'Введіть нову назву:', currentTable.Name, (val) => {
        currentTable.Name = val;
        markAsModified(true);
        renderDatabaseUI();
    });
}

function deleteCurrentTable() {
    if (!currentTable) return;
    if (confirm(`Видалити таблицю '${currentTable.Name}'?`)) {
        const idx = currentDatabase.Tables.findIndex(t => t.Name === currentTable.Name);
        currentDatabase.Tables.splice(idx, 1);
        currentTable = null;
        markAsModified(true);
        renderDatabaseUI();
    }
}

function showInputDialog(title, label, defaultVal, callback) {
    document.getElementById('inputModalTitle').innerText = title;
    document.getElementById('inputModalLabel').innerText = label;
    document.getElementById('inputModalValue').value = defaultVal;

    const btn = document.getElementById('inputModalSaveBtn');
    btn.onclick = () => {
        const val = document.getElementById('inputModalValue').value.trim();
        if (val) {
            callback(val);
            inputModal.hide();
        }
    };

    inputModal.show();
}

function logout() {
    localStorage.removeItem('currentUser');
    window.location.href = 'login.html';
}