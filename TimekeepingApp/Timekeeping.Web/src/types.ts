export interface Project {
  id: string
  name: string
  code: string
  isActive: boolean
}

export interface TaskItem {
  id: string
  name: string
  projectId: string
  isActive: boolean
}

export interface TimeEntry {
  id: string
  taskId: string
  date: string
  hours: number
  description: string
}
