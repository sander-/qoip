import axios from '../lib/axios/axios.min.js';
import { createApp, defineComponent } from '../lib/vue/vue.esm-browser.js';
import { AxiosResponse } from 'axios';

const axiosClient: any = axios;

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
                dnsCopied: false,
                target: '',
                maxHops: 30,
                timeout: 5000,
                resolveDns: false,
                tracerouteResponse: null as TracerouteResponse | null,
                loadingTraceroute: false,
                tracerouteError: null as string | null,
                tracerouteCopied: false
            };
        },
        methods: {
            async performDnsRequest(this: any) {
                this.loading = true;
                this.error = null;
                this.dnsResponse = null;
                try {
                    const response: AxiosResponse<DnsResponse> = await axiosClient.get(`/api/NetworkConnectivity/dns`, {
                        params: {
                            domain: this.domain
                        }
                    });
                    this.dnsResponse = response.data;
                    this.dnsCopied = false;
                } catch (error) {
                    console.error('Error performing DNS request:', error);
                    this.error = 'Error performing DNS request. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            async performTracerouteRequest(this: any) {
                this.loadingTraceroute = true;
                this.tracerouteError = null;
                this.tracerouteResponse = null;
                try {
                    const response: AxiosResponse<TracerouteResponse> = await axiosClient.get(`/api/NetworkConnectivity/traceroute`, {
                        params: {
                            host: this.target,
                            maxHops: this.maxHops,
                            timeout: this.timeout,
                            resolveDns: this.resolveDns
                        }
                    });
                    this.tracerouteResponse = response.data;
                    this.tracerouteCopied = false;
                } catch (error) {
                    console.error('Error performing traceroute request:', error);
                    this.tracerouteError = 'Error performing traceroute request. Please try again.';
                } finally {
                    this.loadingTraceroute = false;
                }
            },
            async copyDnsResponse(this: any) {
                if (!this.dnsResponse) {
                    return;
                }

                await navigator.clipboard.writeText(this.formatJson(this.dnsResponse));
                this.dnsCopied = true;
            },
            async copyTracerouteResponse(this: any) {
                if (!this.tracerouteResponse) {
                    return;
                }

                await navigator.clipboard.writeText(this.formatJson(this.tracerouteResponse));
                this.tracerouteCopied = true;
            },
            formatJson(value: unknown) {
                return JSON.stringify(value, null, 2);
            },
            clearForm(this: any) {
                this.domain = '';
                this.dnsResponse = null;
                this.error = null;
                this.dnsCopied = false;
            },
            clearTracerouteForm(this: any) {
                this.target = '';
                this.maxHops = 30;
                this.timeout = 5000;
                this.resolveDns = false;
                this.tracerouteResponse = null;
                this.tracerouteError = null;
                this.tracerouteCopied = false;
            }
        }
    } as any)
);

app.mount('#app');
