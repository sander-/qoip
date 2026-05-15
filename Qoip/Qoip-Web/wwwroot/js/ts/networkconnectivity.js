var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import axios from '../lib/axios/axios.min.js';
import { createApp, defineComponent } from '../lib/vue/vue.esm-browser.js';
const axiosClient = axios;
const app = createApp(defineComponent({
    data() {
        return {
            domain: '',
            dnsResponse: null,
            loading: false,
            error: null,
            dnsCopied: false,
            target: '',
            maxHops: 30,
            timeout: 5000,
            resolveDns: false,
            tracerouteResponse: null,
            loadingTraceroute: false,
            tracerouteError: null,
            tracerouteCopied: false
        };
    },
    methods: {
        performDnsRequest() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loading = true;
                this.error = null;
                this.dnsResponse = null;
                try {
                    const response = yield axiosClient.get(`/api/NetworkConnectivity/dns`, {
                        params: {
                            domain: this.domain
                        }
                    });
                    this.dnsResponse = response.data;
                    this.dnsCopied = false;
                }
                catch (error) {
                    console.error('Error performing DNS request:', error);
                    this.error = 'Error performing DNS request. Please try again.';
                }
                finally {
                    this.loading = false;
                }
            });
        },
        performTracerouteRequest() {
            return __awaiter(this, void 0, void 0, function* () {
                this.loadingTraceroute = true;
                this.tracerouteError = null;
                this.tracerouteResponse = null;
                try {
                    const response = yield axiosClient.get(`/api/NetworkConnectivity/traceroute`, {
                        params: {
                            host: this.target,
                            maxHops: this.maxHops,
                            timeout: this.timeout,
                            resolveDns: this.resolveDns
                        }
                    });
                    this.tracerouteResponse = response.data;
                    this.tracerouteCopied = false;
                }
                catch (error) {
                    console.error('Error performing traceroute request:', error);
                    this.tracerouteError = 'Error performing traceroute request. Please try again.';
                }
                finally {
                    this.loadingTraceroute = false;
                }
            });
        },
        copyDnsResponse() {
            return __awaiter(this, void 0, void 0, function* () {
                if (!this.dnsResponse) {
                    return;
                }
                yield navigator.clipboard.writeText(this.formatJson(this.dnsResponse));
                this.dnsCopied = true;
            });
        },
        copyTracerouteResponse() {
            return __awaiter(this, void 0, void 0, function* () {
                if (!this.tracerouteResponse) {
                    return;
                }
                yield navigator.clipboard.writeText(this.formatJson(this.tracerouteResponse));
                this.tracerouteCopied = true;
            });
        },
        formatJson(value) {
            return JSON.stringify(value, null, 2);
        },
        clearForm() {
            this.domain = '';
            this.dnsResponse = null;
            this.error = null;
            this.dnsCopied = false;
        },
        clearTracerouteForm() {
            this.target = '';
            this.maxHops = 30;
            this.timeout = 5000;
            this.resolveDns = false;
            this.tracerouteResponse = null;
            this.tracerouteError = null;
            this.tracerouteCopied = false;
        }
    }
}));
app.mount('#app');
