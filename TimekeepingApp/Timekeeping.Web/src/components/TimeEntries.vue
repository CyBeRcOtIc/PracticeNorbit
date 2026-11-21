<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import { api } from '../api'
import type { TaskItem, TimeEntry } from '../types'

const entries = ref<TimeEntry[]>([])
const tasks = ref<TaskItem[]>([])
const error = ref('')

const filterMode = ref<'all' | 'date' | 'month'>('all')
const filterDate = ref('')
const filterYear = ref(new Date().getFullYear())
const filterMonth = ref(new Date().getMonth() + 1)

const summary = ref<{ totalHours: number; status: string } | null>(null)

const newEntry = reactive({ taskId: '', date: '', hours: 1, description: '' })
const editingId = ref<string | null>(null)
const editForm = reactive({ taskId: '', date: '', hours: 1, description: '' })

function activeTasks() {
  return tasks.value.filter((t) => t.isActive)
}

function taskName(id: string) {
  return tasks.value.find((t) => t.id === id)?.name ?? '—'
}

function isTaskActive(id: string) {
  return tasks.value.find((t) => t.id === id)?.isActive ?? false
}

let loadRequestId = 0

async function loadEntries() {
  const requestId = ++loadRequestId
  error.value = ''
  try {
    let loadedEntries: TimeEntry[]
    let loadedSummary: typeof summary.value = null

    if (filterMode.value === 'date' && filterDate.value) {
      loadedEntries = await api.get(`/time-entries?date=${filterDate.value}`)
      loadedSummary = await api.get(`/time-entries/daily-summary?date=${filterDate.value}`)
    } else if (filterMode.value === 'month') {
      loadedEntries = await api.get(`/time-entries?year=${filterYear.value}&month=${filterMonth.value}`)
    } else {
      loadedEntries = await api.get('/time-entries')
    }

    if (requestId !== loadRequestId) return
    entries.value = loadedEntries
    summary.value = loadedSummary
  } catch (e) {
    if (requestId !== loadRequestId) return
    error.value = (e as Error).message
  }
}

async function loadTasks() {
  tasks.value = await api.get('/tasks')
}

function stickerColor(status: string) {
  if (status === 'Under') return 'yellow'
  if (status === 'Exact') return 'green'
  return 'red'
}

async function addEntry() {
  error.value = ''
  try {
    await api.post('/time-entries', newEntry)
    newEntry.taskId = ''
    newEntry.date = ''
    newEntry.hours = 1
    newEntry.description = ''
    await loadEntries()
  } catch (e) {
    error.value = (e as Error).message
  }
}

function startEdit(entry: TimeEntry) {
  editingId.value = entry.id
  editForm.taskId = entry.taskId
  editForm.date = entry.date
  editForm.hours = entry.hours
  editForm.description = entry.description
}

function cancelEdit() {
  editingId.value = null
}

async function saveEdit(id: string) {
  error.value = ''
  try {
    await api.put(`/time-entries/${id}`, editForm)
    editingId.value = null
    await loadEntries()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function deleteEntry(id: string) {
  error.value = ''
  try {
    await api.delete(`/time-entries/${id}`)
    await loadEntries()
  } catch (e) {
    error.value = (e as Error).message
  }
}

watch([filterMode, filterDate, filterYear, filterMonth], loadEntries)

onMounted(async () => {
  await loadTasks()
  await loadEntries()
})
</script>

<template>
  <h2>Проводки</h2>
  <p v-if="error" class="error">{{ error }}</p>

  <form @submit.prevent="addEntry">
    <select v-model="newEntry.taskId" required>
      <option value="" disabled>Задача</option>
      <option v-for="task in activeTasks()" :key="task.id" :value="task.id">{{ task.name }}</option>
    </select>
    <input v-model="newEntry.date" type="date" required />
    <input v-model.number="newEntry.hours" type="number" min="0.01" max="24" step="0.01" required />
    <input v-model="newEntry.description" placeholder="Описание" required />
    <button type="submit">Добавить</button>
  </form>

  <div class="filters">
    <label><input v-model="filterMode" type="radio" value="all" /> За всё время</label>
    <label><input v-model="filterMode" type="radio" value="date" /> За день</label>
    <input v-if="filterMode === 'date'" v-model="filterDate" type="date" />
    <label><input v-model="filterMode" type="radio" value="month" /> За месяц</label>
    <template v-if="filterMode === 'month'">
      <input v-model.number="filterYear" type="number" />
      <input v-model.number="filterMonth" type="number" min="1" max="12" />
    </template>
  </div>

  <p v-if="summary">
    Сумма часов за день: {{ summary.totalHours }}
    <span class="sticker" :class="stickerColor(summary.status)"></span>
  </p>

  <table>
    <thead>
      <tr>
        <th>Дата</th>
        <th>Задача</th>
        <th>Часы</th>
        <th>Описание</th>
        <th></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in entries" :key="entry.id">
        <template v-if="editingId === entry.id">
          <td><input v-model="editForm.date" type="date" /></td>
          <td>
            <select v-model="editForm.taskId" :disabled="!isTaskActive(entry.taskId)">
              <option v-for="task in activeTasks()" :key="task.id" :value="task.id">{{ task.name }}</option>
            </select>
          </td>
          <td><input v-model.number="editForm.hours" type="number" min="0.01" max="24" step="0.01" /></td>
          <td><input v-model="editForm.description" /></td>
          <td>
            <button @click="saveEdit(entry.id)">Сохранить</button>
            <button @click="cancelEdit">Отмена</button>
          </td>
        </template>
        <template v-else>
          <td>{{ entry.date }}</td>
          <td>{{ taskName(entry.taskId) }}</td>
          <td>{{ entry.hours }}</td>
          <td>{{ entry.description }}</td>
          <td>
            <button @click="startEdit(entry)">Изменить</button>
            <button @click="deleteEntry(entry.id)">Удалить</button>
          </td>
        </template>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
.sticker {
  display: inline-block;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  vertical-align: middle;
}
.sticker.yellow { background: #e6c200; }
.sticker.green { background: #2e9e3e; }
.sticker.red { background: #d93025; }
.filters {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin: 1rem 0;
}
</style>
