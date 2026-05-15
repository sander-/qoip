import axios from '../lib/axios/axios.min.js';
import { createApp, defineComponent } from '../lib/vue/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

const axiosClient: any = axios;

interface ApiResponse {
    status: string;
    data: any;
    message: string;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                domain: '',
                dkimSelector: 'default',
                dnsServer: '',
                timeout: 5000,
                response: null as ApiResponse | null,
                loading: false,
                error: null as string | null,
                copied: false
            };
        },
        methods: {
            async performRequest(this: any) {
                this.loading = true;
                this.error = null;
                this.response = null;
                try {
                    const response: AxiosResponse<ApiResponse> = await axiosClient.get('/api/NetworkSecurity/email-security', {
                        params: {
                            domain: this.domain,
                            dkimSelector: this.dkimSelector,
                            dnsServer: this.dnsServer || undefined,
                            timeout: this.timeout
                        }
                    });
                    this.response = response.data;
                    this.copied = false;
                } catch (error) {
                    console.error('Error performing email security analysis:', error);
                    this.error = 'Error performing email security analysis. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            async copyResponse(this: any) {
                if (!this.response) {
                    return;
                }

                await navigator.clipboard.writeText(this.formatJson(this.response));
                this.copied = true;
            },
            formatJson(value: unknown) {
                return JSON.stringify(value, null, 2);
            },
            clearForm(this: any) {
                this.domain = '';
                this.dkimSelector = 'default';
                this.dnsServer = '';
                this.timeout = 5000;
                this.response = null;
                this.error = null;
                this.copied = false;
            }
        }
    } as any)
);

app.mount('#app');
