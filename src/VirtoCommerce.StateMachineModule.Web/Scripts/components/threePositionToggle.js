class ThreePositionToggle {
    constructor(container, options = {}) {
        this.container = container;
        this.options = {
            onPositionChange: options.onPositionChange || (() => {}),
            initialPosition: options.initialPosition || 'middle',
            leftLabel: options.leftLabel || '',
            middleLabel: options.middleLabel || '',
            rightLabel: options.rightLabel || '',
            leftIcon: options.leftIcon || '',
            middleIcon: options.middleIcon || '',
            rightIcon: options.rightIcon || '',
            leftColor: options.leftColor || '#4CAF50',
            middleColor: options.middleColor || '#666',
            rightColor: options.rightColor || '#f44336',
            width: options.width || 54,
            height: options.height || 16
        };
        this.render();
        this.attachEvents();
    }

    render() {
        const html = `
            <div class="toggle-labels">
                <span class="toggle-label">${this.options.leftLabel}</span>
                <div class="three-position-toggle">
                    <div class="toggle-option">
                        ${this.options.leftIcon ? `<i class="${this.options.leftIcon}"></i>` : ''}
                    </div>
                    <div class="toggle-option">
                        ${this.options.middleIcon ? `<i class="${this.options.middleIcon}"></i>` : ''}
                    </div>
                    <div class="toggle-option">
                        ${this.options.rightIcon ? `<i class="${this.options.rightIcon}"></i>` : ''}
                    </div>
                    <div class="toggle-slider ${this.options.initialPosition}"></div>
                </div>
                <span class="toggle-label">${this.options.rightLabel}</span>
            </div>
        `;
        this.container.innerHTML = html;

        // Add styles if they don't exist
        if (!document.getElementById('three-position-toggle-styles')) {
            const styles = document.createElement('style');
            styles.id = 'three-position-toggle-styles';
            styles.textContent = `
                .three-position-toggle {
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    gap: 4px;
                    padding: 1px;
                    background: #333;
                    border-radius: 3px;
                    width: ${this.options.width}px;
                    height: ${this.options.height}px;
                    margin: 0;
                    position: relative;
                    cursor: pointer;
                }

                .toggle-option {
                    flex: 1;
                    text-align: center;
                    z-index: 1;
                    color: white;
                    font-size: 10px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    height: ${this.options.height - 2}px;
                    border-radius: 2px;
                    cursor: pointer;
                }

                .toggle-slider {
                    position: absolute;
                    width: calc(33.33% - 2px);
                    height: calc(100% - 2px);
                    border-radius: 2px;
                    transition: all 0.2s ease;
                    top: 1px;
                }

                .toggle-slider.left {
                    background: ${this.options.leftColor};
                    left: 1px;
                }

                .toggle-slider.middle {
                    background: ${this.options.middleColor};
                    left: calc(33.33% + 1px);
                }

                .toggle-slider.right {
                    background: ${this.options.rightColor};
                    left: calc(66.66% + 1px);
                }

                .toggle-labels {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    gap: 8px;
                    width: 100%;
                    margin: 4px auto;
                    font-size: 10px;
                    color: #666;
                }

                .toggle-label {
                    cursor: pointer;
                }
            `;
            document.head.appendChild(styles);
        }
    }

    attachEvents() {
        const toggle = this.container.querySelector('.three-position-toggle');
        const leftLabel = this.container.querySelector('.toggle-label:first-child');
        const rightLabel = this.container.querySelector('.toggle-label:last-child');

        if (this.options.leftLabel) {
            leftLabel.addEventListener('click', (e) => {
                e.stopPropagation();
                e.preventDefault();
                this.setPosition('left');
            });
        }

        if (this.options.rightLabel) {
            rightLabel.addEventListener('click', (e) => {
                e.stopPropagation();
                e.preventDefault();
                this.setPosition('right');
            });
        }

        toggle.addEventListener('mousedown', (e) => this.startToggleSlide(e));
    }

    startToggleSlide(event) {
        event.stopPropagation();
        const toggle = event.currentTarget;
        const toggleRect = toggle.getBoundingClientRect();
        const optionWidth = toggleRect.width / 3;
        let isSliding = false;
        
        const relativeX = event.clientX - toggleRect.left;
        
        const handleSlide = (e) => {
            e.stopPropagation();
            isSliding = true;
            const relativeX = e.clientX - toggleRect.left;
            const position = Math.min(Math.max(0, relativeX), toggleRect.width);
            
            if (position < optionWidth) {
                this.setPosition('left');
            } else if (position < optionWidth * 2) {
                this.setPosition('middle');
            } else {
                this.setPosition('right');
            }
        };

        const stopSlide = (e) => {
            e.stopPropagation();
            document.removeEventListener('mousemove', handleSlide);
            document.removeEventListener('mouseup', stopSlide);
            
            if (!isSliding) {
                if (relativeX < optionWidth) {
                    this.setPosition('left');
                } else if (relativeX < optionWidth * 2) {
                    this.setPosition('middle');
                } else {
                    this.setPosition('right');
                }
            }
        };

        document.addEventListener('mousemove', handleSlide);
        document.addEventListener('mouseup', stopSlide);
    }

    setPosition(position) {
        const slider = this.container.querySelector('.toggle-slider');
        slider.className = 'toggle-slider ' + position;
        this.options.onPositionChange(position);
    }

    getPosition() {
        const slider = this.container.querySelector('.toggle-slider');
        return slider.className.replace('toggle-slider ', '');
    }
}

// Export the component
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ThreePositionToggle;
} else {
    window.ThreePositionToggle = ThreePositionToggle;
} 