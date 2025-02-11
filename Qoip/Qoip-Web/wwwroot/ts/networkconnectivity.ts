import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { AxiosResponse } from 'axios';

interface DnsResponse {
    status: string;
    data: any;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                domain: '',
                dnsResponse: null as DnsResponse | null,
                loading: false,
                error: null as string | null
            };
        },
        methods: {
            async performDnsRequest() {
                this.loading = true;
                this.error = null;
                this.dnsResponse = null;
                try {
                    const response: AxiosResponse<DnsResponse> = await axios.get(`/api/NetworkConnectivity/dns`, {
                        params: {
                            domain: this.domain
                        }
                    });
                    this.dnsResponse = response.data;
                } catch (error) {
                    console.error('Error performing DNS request:', error);
                    this.error = 'Error performing DNS request. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            clearForm() {
                this.domain = '';
                this.dnsResponse = null;
                this.error = null;
            }
        }
    })
);

app.mount('#app');
