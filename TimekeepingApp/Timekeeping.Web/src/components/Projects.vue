<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { api } from '../api'
import type { Project } from '../types'

const projects = ref<Project[]>([])
const error = ref('')
const newProject = reactive({ name: '', code: '', isActive: true })
const editingId = ref<string | null>(null)
const editForm = reactive({ name: '', code: '', isActive: true })

async function loadProjects() {
  error.value = ''
  try {
    projects.value = await api.get('/projects')
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function addProject() {
  error.value = ''
  try {
    await api.post('/projects', newProject)
    newProject.name = ''
    newProject.code = ''
    newProject.isActive = true
    await loadProjects()
  } catch (e) {
    error.value = (e as Error).message
  }
}

function startEdit(project: Project) {
  editingId.value = project.id
  editForm.name = project.name
  editForm.code = project.code
  editForm.isActive = project.isActive
}

function cancelEdit() {
  editingId.value = null
}

async function saveEdit(id: string) {
  error.value = ''
  try {
    await api.put(`/projects/${id}`, editForm)
    editingId.value = null
    await loadProjects()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function deleteProject(id: string) {
  error.value = ''
  try {
    await api.delete(`/projects/${id}`)
    await loadProjects()
  } catch (e) {
    error.value = (e as Error).message
  }
}

onMounted(loadProjects)
</script>

<template>
  <h2>Проекты</h2>
  <p v-if="error" class="error">{{ error }}</p>

  <form @submit.prevent="addProject">
    <input v-model="newProject.name" placeholder="Название" required />
    <input v-model="newProject.code" placeholder="Код" required />
    <label><input v-model="newProject.isActive" type="checkbox" /> Активный</label>
    <button type="submit">Добавить</button>
  </form>

  <table>
    <thead>
      <tr>
        <th>Название</th>
        <th>Код</th>
        <th>Активный</th>
        <th></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="project in projects" :key="project.id">
        <template v-if="editingId === project.id">
          <td><input v-model="editForm.name" /></td>
          <td><input v-model="editForm.code" /></td>
          <td><input v-model="editForm.isActive" type="checkbox" /></td>
          <td>
            <button @click="saveEdit(project.id)">Сохранить</button>
            <button @click="cancelEdit">Отмена</button>
          </td>
        </template>
        <template v-else>
          <td>{{ project.name }}</td>
          <td>{{ project.code }}</td>
          <td>{{ project.isActive ? 'Да' : 'Нет' }}</td>
          <td>
            <button @click="startEdit(project)">Изменить</button>
            <button @click="deleteProject(project.id)">Удалить</button>
          </td>
        </template>
      </tr>
    </tbody>
  </table>
</template>
