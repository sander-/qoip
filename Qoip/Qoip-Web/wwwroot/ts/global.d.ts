declare module 'https://unpkg.com/vue@3/dist/vue.esm-browser.js' {
    const Vue: typeof import('vue');
    export * from 'vue';
}

declare module 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js' {
    const axios: typeof import('axios');
    export default axios;
}