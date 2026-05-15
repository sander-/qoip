import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

interface ApiResponse {
    status: string;
    data: any;
    message: string;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                url: '',
                timeout: 5000,
                response: null as ApiResponse | null,
                loading: false,
                error: null as string | null
            };
        },
        methods: {
            async performRequest() {
                this.loading = true;
                this.error = null;
                this.response = null;
                try {
                    const response: AxiosResponse<ApiResponse> = await axios.get('/api/NetworkConnectivity/http-probe', {
                        params: {
                            url: this.url,
                            timeout: this.timeout
                        }
                    });
                    this.response = response.data;
                } catch (error) {
                    console.error('Error performing HTTP probe:', error);
                    this.error = 'Error performing HTTP probe. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            clearForm() {
                this.url = '';
                this.timeout = 5000;
                this.response = null;
                this.error = null;
            }
        }
    })
);

app.mount('#app');
