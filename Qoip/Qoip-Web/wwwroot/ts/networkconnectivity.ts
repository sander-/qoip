import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

interface DnsResponse {
    status: string;
    data: any;
}

interface TracerouteResponse {
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
                error: null as string | null,
                target: '',
                maxHops: 30,
                timeout: 5000,
                resolveDns: false,
                tracerouteResponse: null as TracerouteResponse | null,
                loadingTraceroute: false,
                tracerouteError: null as string | null
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
            async performTracerouteRequest() {
                this.loadingTraceroute = true;
                this.tracerouteError = null;
                this.tracerouteResponse = null;
                try {
                    const response: AxiosResponse<TracerouteResponse> = await axios.get(`/api/NetworkConnectivity/traceroute`, {
                        params: {
                            host: this.target,
                            maxHops: this.maxHops,
                            timeout: this.timeout,
                            resolveDns: this.resolveDns
                        }
                    });
                    this.tracerouteResponse = response.data;
                } catch (error) {
                    console.error('Error performing traceroute request:', error);
                    this.tracerouteError = 'Error performing traceroute request. Please try again.';
                } finally {
                    this.loadingTraceroute = false;
                }
            },
            clearForm() {
                this.domain = '';
                this.dnsResponse = null;
                this.error = null;
            },
            clearTracerouteForm() {
                this.target = '';
                this.maxHops = 30;
                this.timeout = 5000;
                this.resolveDns = false;
                this.tracerouteResponse = null;
                this.tracerouteError = null;
            }
        }
    })
);

app.mount('#app');
