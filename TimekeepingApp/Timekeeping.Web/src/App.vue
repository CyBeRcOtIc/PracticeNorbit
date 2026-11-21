<script setup lang="ts">
import { ref } from 'vue'
import Projects from './components/Projects.vue'
import Tasks from './components/Tasks.vue'
import TimeEntries from './components/TimeEntries.vue'

const tab = ref<'projects' | 'tasks' | 'entries'>('projects')
</script>

<template>
  <h1>Учёт рабочего времени</h1>

  <nav>
    <button @click="tab = 'projects'" :class="{ active: tab === 'projects' }">Проекты</button>
    <button @click="tab = 'tasks'" :class="{ active: tab === 'tasks' }">Задачи</button>
    <button @click="tab = 'entries'" :class="{ active: tab === 'entries' }">Проводки</button>
  </nav>

  <Projects v-if="tab === 'projects'" />
  <Tasks v-else-if="tab === 'tasks'" />
  <TimeEntries v-else-if="tab === 'entries'" />
</template>
