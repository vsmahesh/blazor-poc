// Read configuration from inline script
const config = window.SESSION_TIMEOUT_CONFIG || { timeoutMinutes: 5, warningBeforeMinutes: 1 };
const SESSION_TIMEOUT_MS = config.timeoutMinutes * 60 * 1000;
const WARNING_BEFORE_MS = config.warningBeforeMinutes * 60 * 1000;
const COUNTDOWN_SECONDS = config.warningBeforeMinutes * 60;

// State variables
let warningTimer = null;
let countdownTimer = null;
let countdownValue = COUNTDOWN_SECONDS;
let isModalShown = false;

// DOM elements
const modal = document.getElementById('session-timeout-modal');
const extendButton = document.getElementById('session-extend-button');
const countdownElement = document.getElementById('session-timeout-countdown');

// Check if user is authenticated
function isUserAuthenticated() {
    const currentPath = window.location.pathname.toLowerCase();
    // Skip timeout tracking on public pages
    if (currentPath === '/login' || currentPath === '/logout' || currentPath === '/access-denied') {
        return false;
    }
    return true;
}

// Start warning timer (triggers modal at configured time before session expires)
function startWarningTimer() {
    // Clear any existing timer
    if (warningTimer) {
        clearTimeout(warningTimer);
    }

    // Calculate time until warning should appear
    const timeUntilWarning = SESSION_TIMEOUT_MS - WARNING_BEFORE_MS;

    // Set timer to show warning modal
    warningTimer = setTimeout(showWarningModal, timeUntilWarning);
}

// Show warning modal
function showWarningModal() {
    if (!modal) {
        console.error('Session timeout modal not found');
        return;
    }

    // Prevent dismissal via ESC key
    modal.addEventListener('cancel', preventDismiss);

    isModalShown = true;
    countdownValue = COUNTDOWN_SECONDS;
    updateCountdownDisplay();

    modal.showModal();

    // Start countdown
    startCountdown();
}

// Prevent modal dismissal
function preventDismiss(event) {
    event.preventDefault();
}

// Start countdown timer
function startCountdown() {
    // Clear any existing countdown
    if (countdownTimer) {
        clearInterval(countdownTimer);
    }

    countdownTimer = setInterval(() => {
        countdownValue--;
        updateCountdownDisplay();

        if (countdownValue <= 0) {
            clearInterval(countdownTimer);
            handleSessionExpired();
        }
    }, 1000);
}

// Update countdown display
function updateCountdownDisplay() {
    if (countdownElement) {
        countdownElement.textContent = countdownValue;
    }
}

// Handle session expiration (auto-logout)
function handleSessionExpired() {
    // Redirect to logout endpoint which clears the HttpOnly cookie
    window.location.href = '/logout?expired=true';
}

// Extend session (keep-alive API call)
async function extendSession() {
    if (!extendButton) return;

    // Disable button during request
    extendButton.disabled = true;
    extendButton.textContent = 'Extending...';

    try {
        const response = await fetch('/api/session/keep-alive', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'same-origin', // Include cookies
        });

        if (!response.ok) {
            // If unauthorized, redirect to login
            if (response.status === 401) {
                window.location.href = '/login?expired=true';
                return;
            }
            throw new Error(`Failed to extend session: ${response.status}`);
        }

        // Success - close modal and reset timers
        closeModalAndReset();

    } catch (error) {
        console.error('Error extending session:', error);

        // Show error state
        extendButton.textContent = 'Network Error - Retry';
        extendButton.disabled = false;

        // Reset button text after 3 seconds
        setTimeout(() => {
            if (extendButton && !extendButton.disabled) {
                extendButton.textContent = 'Extend Session';
            }
        }, 3000);
    }
}

// Close modal and reset all timers
function closeModalAndReset() {
    if (!modal) return;

    // Stop countdown
    if (countdownTimer) {
        clearInterval(countdownTimer);
    }

    // Close modal
    modal.close();
    isModalShown = false;

    // Reset countdown value
    countdownValue = COUNTDOWN_SECONDS;

    // Reset warning timer (starts fresh 4-minute countdown)
    startWarningTimer();

    // Re-enable button
    if (extendButton) {
        extendButton.disabled = false;
        extendButton.textContent = 'Extend Session';
    }
}

// Cleanup timers
function cleanup() {
    if (warningTimer) {
        clearTimeout(warningTimer);
    }
    if (countdownTimer) {
        clearInterval(countdownTimer);
    }
}

// Initialize on DOM load
if (isUserAuthenticated()) {
    if (extendButton) {
        extendButton.addEventListener('click', extendSession);
    }

    // Start warning timer when page loads (session is fresh at page load)
    startWarningTimer();

    // Cleanup on page unload
    window.addEventListener('beforeunload', cleanup);
} else {
    console.log('Session timeout tracking disabled (unauthenticated page)');
}
