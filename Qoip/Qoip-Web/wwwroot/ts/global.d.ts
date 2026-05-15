declare module 'https://unpkg.com/vue@3/dist/vue.esm-browser.js' {
    export * from 'vue';
}

declare module 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js' {
    import axios from 'axios';
    export default axios;
}

declare module '../lib/vue/vue.esm-browser.js' {
    export * from 'vue';
}

declare module '../lib/axios/axios.min.js' {
    import axios from 'axios';
    export default axios;
}
