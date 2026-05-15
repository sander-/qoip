import { createApp, defineComponent } from '../lib/vue/vue.esm-browser.js';
import axios from '../lib/axios/axios.min.js';
import { AxiosResponse } from 'axios';

const axiosClient: any = axios;

interface CertificateValidationResponse {
    issuedTo: string;
    issuedBy: string;
    validityPeriod: string;
    fingerprints: string;
    version: number;
    algorithm: string;
    usage: string;
    alternativeNames: string[];
    extensions: Record<string, string[]>;
    validFrom: string;
    validTo: string;
}

const app = createApp(
    defineComponent({
        data() {
            return {
                url: '',
                expirationWarningThresholdInDays: 0,
                validationResponse: null as CertificateValidationResponse | null,
                loading: false,
                error: null as string | null,
                copied: false
            };
        },
        methods: {
            async performCertificateValidation(this: any) {
                this.loading = true;
                this.error = null;
                this.validationResponse = null;
                try {
                    const response: AxiosResponse<CertificateValidationResponse> = await axiosClient.get(`/api/securityencryption/certificate`, {
                        params: {
                            url: this.url,
                            expirationWarningThresholdInDays: this.expirationWarningThresholdInDays
                        }
                    });
                    this.validationResponse = response.data;
                    this.copied = false;
                } catch (error) {
                    console.error('Error performing certificate validation:', error);
                    this.error = 'Error performing certificate validation. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            async copyValidationResponse(this: any) {
                if (!this.validationResponse) {
                    return;
                }

                await navigator.clipboard.writeText(this.formatJson(this.validationResponse));
                this.copied = true;
            },
            formatJson(value: unknown) {
                return JSON.stringify(value, null, 2);
            },
            clearForm(this: any) {
                this.url = '';
                this.expirationWarningThresholdInDays = 0;
                this.validationResponse = null;
                this.error = null;
                this.copied = false;
            }
        }
    } as any)
);

app.mount('#app');
