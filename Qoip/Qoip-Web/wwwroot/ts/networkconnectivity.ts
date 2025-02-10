import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { AxiosResponse } from 'axios';

interface DnsResponse {
    // Define the structure of the DNS response here
    // Example:
    status: string;
    data: any;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                domain: '',
                dnsResponse: null as DnsResponse | null
            };
        },
        methods: {
            async performDnsRequest() {
                try {
                    const response: AxiosResponse<DnsResponse> = await axios.get(`/api/NetworkConnectivity/dns`, {
                        params: {
                            domain: this.domain
                        }
                    });
                    this.dnsResponse = response.data;
                } catch (error) {
                    console.error('Error performing DNS request:', error);
                    this.dnsResponse = { status: 'error', data: 'Error performing DNS request. Please try again.' };
                }
            }
        }
    })
);

app.mount('#app');
