<template>
  <v-app class="app-shell">
    <v-app-bar class="app-bar" color="transparent" flat height="64">
      <v-toolbar-title class="app-title">
        <span class="app-title__brand">vSphere VM Exporter</span>
        <span class="app-title__tag">VM 匯出控管中心</span>
      </v-toolbar-title>
    </v-app-bar>
    <v-main class="main-content">
      <router-view />
    </v-main>
  </v-app>
</template>
