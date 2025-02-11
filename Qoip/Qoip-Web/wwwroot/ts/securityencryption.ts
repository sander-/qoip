import { createApp, defineComponent } from 'https://unpkg.com/vue@3/dist/vue.esm-browser.js';
import axios from 'https://cdn.jsdelivr.net/npm/axios/dist/esm/axios.min.js';
import { AxiosResponse } from 'axios';

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
                error: null as string | null
            };
        },
        methods: {
            async performCertificateValidation() {
                this.loading = true;
                this.error = null;
                this.validationResponse = null;
                try {
                    const response: AxiosResponse<CertificateValidationResponse> = await axios.get(`/api/securityencryption/certificate`, {
                        params: {
                            url: this.url,
                            expirationWarningThresholdInDays: this.expirationWarningThresholdInDays
                        }
                    });
                    this.validationResponse = response.data;
                } catch (error) {
                    console.error('Error performing certificate validation:', error);
                    this.error = 'Error performing certificate validation. Please try again.';
                } finally {
                    this.loading = false;
                }
            },
            clearForm() {
                this.url = '';
                this.expirationWarningThresholdInDays = 0;
                this.validationResponse = null;
                this.error = null;
            }
        }
    })
);

app.mount('#app');
