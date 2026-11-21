<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { api } from '../api'
import type { Project, TaskItem } from '../types'

const tasks = ref<TaskItem[]>([])
const projects = ref<Project[]>([])
const error = ref('')
const newTask = reactive({ name: '', projectId: '', isActive: true })
const editingId = ref<string | null>(null)
const editForm = reactive({ name: '', projectId: '', isActive: true })

function projectName(id: string) {
  return projects.value.find((p) => p.id === id)?.name ?? '—'
}

async function loadAll() {
  error.value = ''
  try {
    projects.value = await api.get('/projects')
    tasks.value = await api.get('/tasks')
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function addTask() {
  error.value = ''
  try {
    await api.post('/tasks', newTask)
    newTask.name = ''
    newTask.projectId = ''
    newTask.isActive = true
    await loadAll()
  } catch (e) {
    error.value = (e as Error).message
  }
}

function startEdit(task: TaskItem) {
  editingId.value = task.id
  editForm.name = task.name
  editForm.projectId = task.projectId
  editForm.isActive = task.isActive
}

function cancelEdit() {
  editingId.value = null
}

async function saveEdit(id: string) {
  error.value = ''
  try {
    await api.put(`/tasks/${id}`, editForm)
    editingId.value = null
    await loadAll()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function deleteTask(id: string) {
  error.value = ''
  try {
    await api.delete(`/tasks/${id}`)
    await loadAll()
  } catch (e) {
    error.value = (e as Error).message
  }
}

onMounted(loadAll)
</script>

<template>
  <h2>Задачи</h2>
  <p v-if="error" class="error">{{ error }}</p>

  <form @submit.prevent="addTask">
    <input v-model="newTask.name" placeholder="Название" required />
    <select v-model="newTask.projectId" required>
      <option value="" disabled>Проект</option>
      <option v-for="project in projects" :key="project.id" :value="project.id">{{ project.name }}</option>
    </select>
    <label><input v-model="newTask.isActive" type="checkbox" /> Активный</label>
    <button type="submit">Добавить</button>
  </form>

  <table>
    <thead>
      <tr>
        <th>Название</th>
        <th>Проект</th>
        <th>Активный</th>
        <th></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="task in tasks" :key="task.id">
        <template v-if="editingId === task.id">
          <td><input v-model="editForm.name" /></td>
          <td>
            <select v-model="editForm.projectId">
              <option v-for="project in projects" :key="project.id" :value="project.id">{{ project.name }}</option>
            </select>
          </td>
          <td><input v-model="editForm.isActive" type="checkbox" /></td>
          <td>
            <button @click="saveEdit(task.id)">Сохранить</button>
            <button @click="cancelEdit">Отмена</button>
          </td>
        </template>
        <template v-else>
          <td>{{ task.name }}</td>
          <td>{{ projectName(task.projectId) }}</td>
          <td>{{ task.isActive ? 'Да' : 'Нет' }}</td>
          <td>
            <button @click="startEdit(task)">Изменить</button>
            <button @click="deleteTask(task.id)">Удалить</button>
          </td>
        </template>
      </tr>
    </tbody>
  </table>
</template>
