/**
 * SAGRISA Mobile Framework JavaScript
 * Sistema de funcionalidades mobile-first para SAGRISA
 */

(function () {
    'use strict';

    // Namespace principal
    window.SagrisaApp = {
        // Estado de la aplicación
        state: {
            isMobile: false,
            currentStep: 1,
            formData: {},
            toasts: []
        },

        // Inicialización
        init: function () {
            this.detectDevice();
            this.initEventListeners();
            this.initForms();
            this.initTooltips();
            console.log('🚀 SAGRISA Mobile Framework iniciado');
        },

        // Detección de dispositivo
        detectDevice: function () {
            this.state.isMobile = window.innerWidth <= 768 ||
                /Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

            if (this.state.isMobile) {
                document.body.classList.add('mobile-device');
            }

            return this.state.isMobile;
        },

        // Verificar si es móvil
        isMobile: function () {
            return this.state.isMobile;
        },

        // Event listeners principales
        initEventListeners: function () {
            // Resize handler
            window.addEventListener('resize', () => {
                this.detectDevice();
            });

            // Form submission handler
            document.addEventListener('submit', (e) => {
                const form = e.target;
                if (form.classList.contains('sagrisa-form')) {
                    this.handleFormSubmission(e);
                }
            });

            // Button click handler
            document.addEventListener('click', (e) => {
                const target = e.target;

                // Manejo de botones con loading
                if (target.classList.contains('btn-sagrisa') && target.type === 'submit') {
                    this.showButtonLoading(target);
                }

                // Manejo de cards clickeable
                if (target.closest('.card-clickable')) {
                    this.handleCardClick(target.closest('.card-clickable'));
                }
            });

            // Input handlers para validación en tiempo real
            document.addEventListener('input', (e) => {
                const input = e.target;
                if (input.classList.contains('form-control-sagrisa')) {
                    this.validateInput(input);
                }
            });
        },

        // Inicialización de formularios
        initForms: function () {
            const forms = document.querySelectorAll('.sagrisa-form');
            forms.forEach(form => {
                this.enhanceForm(form);
            });
        },

        // Mejorar formulario
        enhanceForm: function (form) {
            // Añadir clases de validación
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                if (!input.classList.contains('form-control-sagrisa')) {
                    input.classList.add('form-control-sagrisa');
                }
            });

            // Configurar validación HTML5
            form.setAttribute('novalidate', 'true');
        },

        // Validación de input
        validateInput: function (input) {
            const isValid = input.checkValidity();
            const feedbackElement = input.parentNode.querySelector('.invalid-feedback');

            if (!isValid) {
                input.classList.add('is-invalid');
                input.classList.remove('is-valid');

                if (feedbackElement) {
                    feedbackElement.textContent = input.validationMessage;
                    feedbackElement.style.display = 'block';
                }
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');

                if (feedbackElement) {
                    feedbackElement.style.display = 'none';
                }
            }

            return isValid;
        },

        // Manejo de envío de formularios
        handleFormSubmission: function (e) {
            const form = e.target;
            const submitButton = form.querySelector('button[type="submit"]');

            // Validar todos los campos
            const inputs = form.querySelectorAll('.form-control-sagrisa');
            let isFormValid = true;

            inputs.forEach(input => {
                if (!this.validateInput(input)) {
                    isFormValid = false;
                }
            });

            if (!isFormValid) {
                e.preventDefault();
                this.showToast('Por favor, corrige los errores en el formulario', 'error');
                this.hideButtonLoading(submitButton);
                return false;
            }
        },

        // Mostrar loading en botón
        showButtonLoading: function (button) {
            if (!button) return;

            const originalText = button.innerHTML;
            button.setAttribute('data-original-text', originalText);
            button.innerHTML = '<span class="loading-spinner"></span> Procesando...';
            button.disabled = true;
        },

        // Ocultar loading en botón
        hideButtonLoading: function (button) {
            if (!button) return;

            const originalText = button.getAttribute('data-original-text');
            if (originalText) {
                button.innerHTML = originalText;
                button.removeAttribute('data-original-text');
            }
            button.disabled = false;
        },

        // Sistema de notificaciones Toast
        showToast: function (message, type = 'info', duration = 5000) {
            const toastId = 'toast-' + Date.now();
            const toast = this.createToastElement(toastId, message, type);

            const container = document.getElementById('toast-container');
            if (!container) {
                console.warn('Toast container no encontrado');
                return;
            }

            container.appendChild(toast);

            // Mostrar toast
            setTimeout(() => {
                toast.classList.add('show');
            }, 100);

            // Auto-dismiss
            setTimeout(() => {
                this.hideToast(toastId);
            }, duration);

            return toastId;
        },

        // Crear elemento toast
        createToastElement: function (id, message, type) {
            const toast = document.createElement('div');
            toast.id = id;
            toast.className = 'toast-sagrisa';

            const iconMap = {
                success: '✅',
                error: '❌',
                warning: '⚠️',
                info: 'ℹ️'
            };

            const colorMap = {
                success: '#28a745',
                error: '#dc3545',
                warning: '#ffc107',
                info: '#00A0E8'
            };

            toast.innerHTML = `
                <div class="toast-header" style="background-color: ${colorMap[type] || colorMap.info}; color: white;">
                    <span class="toast-icon">${iconMap[type] || iconMap.info}</span>
                    <strong class="toast-title">SAGRISA</strong>
                    <button type="button" class="btn-close btn-close-white ms-auto" onclick="SagrisaApp.hideToast('${id}')"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            `;

            return toast;
        },

        // Ocultar toast
        hideToast: function (toastId) {
            const toast = document.getElementById(toastId);
            if (toast) {
                toast.classList.remove('show');
                setTimeout(() => {
                    toast.remove();
                }, 300);
            }
        },

        // Navegación de wizard
        initWizardNavigation: function () {
            const steps = document.querySelectorAll('.wizard-step');
            const currentStep = parseInt(document.body.dataset.currentStep || '1');

            steps.forEach((step, index) => {
                const stepNumber = index + 1;

                if (stepNumber < currentStep) {
                    step.classList.add('completed');
                } else if (stepNumber === currentStep) {
                    step.classList.add('active');
                }
            });
        },

        // Ir al siguiente paso del wizard
        nextStep: function (currentStepNumber) {
            const form = document.querySelector('.sagrisa-form');
            if (form && !this.validateForm(form)) {
                return false;
            }

            // Aquí puedes añadir lógica para navegar al siguiente paso
            console.log('Avanzando al paso:', currentStepNumber + 1);
            return true;
        },

        // Ir al paso anterior del wizard
        previousStep: function (currentStepNumber) {
            console.log('Retrocediendo al paso:', currentStepNumber - 1);
            return true;
        },

        // Validar formulario completo
        validateForm: function (form) {
            const inputs = form.querySelectorAll('.form-control-sagrisa');
            let isValid = true;

            inputs.forEach(input => {
                if (!this.validateInput(input)) {
                    isValid = false;
                }
            });

            return isValid;
        },

        // Manejo de cards clickeable
        handleCardClick: function (card) {
            const url = card.dataset.href;
            if (url) {
                window.location.href = url;
            }
        },

        // Inicializar tooltips
        initTooltips: function () {
            // Si tienes Bootstrap, puedes usar sus tooltips
            if (window.bootstrap && bootstrap.Tooltip) {
                const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
                tooltips.forEach(tooltip => {
                    new bootstrap.Tooltip(tooltip);
                });
            }
        },

        // Utilidades para animaciones
        animateElement: function (element, animationClass, duration = 500) {
            element.classList.add(animationClass);
            setTimeout(() => {
                element.classList.remove(animationClass);
            }, duration);
        },

        // Scroll suave a elemento
        scrollToElement: function (element, offset = 0) {
            const targetPosition = element.offsetTop - offset;
            window.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
        },

        // Formatear moneda
        formatCurrency: function (amount) {
            return new Intl.NumberFormat('es-SV', {
                style: 'currency',
                currency: 'USD'
            }).format(amount);
        },

        // Formatear fecha
        formatDate: function (date) {
            return new Intl.DateTimeFormat('es-SV', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            }).format(new Date(date));
        },

        // Debounce function para optimización
        debounce: function (func, wait) {
            let timeout;
            return function executedFunction(...args) {
                const later = () => {
                    clearTimeout(timeout);
                    func(...args);
                };
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
            };
        }
    };

    // Auto-inicialización cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            SagrisaApp.init();
        });
    } else {
        SagrisaApp.init();
    }

})();
