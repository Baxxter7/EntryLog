const video = document.getElementById('video');
const snapshotCanvas = document.getElementById('snapshot');
const ctxSnapshot = snapshotCanvas.getContext('2d');
const configButton = document.getElementById("faceid-config-btn");
const circleOverlay = document.getElementById('circle-overlay');
const loader = document.getElementById("loader-overlay");

// 🔧 Variables de estado
var readySnapshot = false;
let countdownActive = false;
let countdownTimer = null;
let detectingFace = false;
// Aqui hice cambios - Umbrales para considerar el rostro "listo"
const minFaceConfidence = 0.4;
const minFaceRadiusFactor = 0.4;
const maxFaceRadiusFactor = 1.1;
// Aqui hice cambios - Control de movimiento durante la cuenta regresiva
let lastFaceCenter = null;
const maxCenterMoveFactor = 0.08;
const maxOverallMoveFactor = 0.12;
// Aqui hice cambios - Estabilidad y tolerancia
let stableSince = null;
let invalidSince = null;
const stabilityMs = 500;
const invalidToleranceMs = 0;
// Aqui hice cambios - Margen interno del circulo
const circleInnerMarginPx = 10;
let countdownStartCenter = null;

const captureModal = document.getElementById('camera-modal');
const captureBootstrapModal = new bootstrap.Modal(captureModal);
const saveFaceIdButton = document.getElementById("save-faceid-button");
const detectorData = document.getElementById("detector-data");


/**
 * Inicia la transmisión de video
 */
async function startVideo() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        circleOverlay.style.display = 'block';
        $("#faceid-helper").removeClass('visually-hidden');
    } catch (err) {
        console.error("❌ [startVideo] No se pudo acceder a la cámara:", err);
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'No se pudo acceder a la cámara. Verifica permisos'
        }, { type: 'danger' });
        inactiveVideoSectionLoader();
        $("#faceid-config-btn").removeClass('pointer-events disabled');
    }
}


/**
 * Detiene la transmisión de video
 */
function stopVideo() {
    const stream = video.srcObject;
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
    }
    video.srcObject = null;
}


/**
 * Detiene el video y resetea el estado sin haber capturado
 */
function stopFaceId() {
    detectingFace = false;

    if (countdownActive) {
        clearInterval(countdownTimer);
        document.getElementById('countdown-overlay').style.display = 'none';
        countdownActive = false;
    }

    readySnapshot = false;
    video.removeEventListener('playing', detectFace);
    stopVideo();

    video.classList.add('visually-hidden');
    circleOverlay.classList.add('visually-hidden');
    circleOverlay.style.borderColor = '';
    $("#faceid-helper").addClass('visually-hidden');
    $("#stop-video-btn-container").hide();
    $("#faceid-config-btn").removeClass('pointer-events disabled');
}


/**
 * Carga los modelos de face-api.js
 */
async function loadModels() {
    try {
        await faceapi.nets.tinyFaceDetector.loadFromUri('/lib/face-api-js/models');
        await faceapi.nets.faceLandmark68Net.loadFromUri('/lib/face-api-js/models');
        await faceapi.nets.faceRecognitionNet.loadFromUri('/lib/face-api-js/models');
    } catch (err) {
        console.error("❌ [loadModels] Error al cargar modelos:", err);
        throw err;
    }
}


/**
 * Activa el loader de video
 */
function activeVideoSectionLoader() {
    loader.style.display = 'flex';
    loader.style.visibility = 'visible';
}


/**
 * Inactiva el loader de video
 */
function inactiveVideoSectionLoader() {
    loader.style.display = 'none';
    loader.style.visibility = 'hidden';
}


/**
 * Verifica que el rostro esté completamente dentro del círculo
 * Aqui hice cambios - Se corrige la validacion usando las esquinas del rostro
 */
function isFaceFullyInsideCircle(box, circleX, circleY, radius) {
    const points = [
        { x: box.x, y: box.y },
        { x: box.x + box.width, y: box.y },
        { x: box.x, y: box.y + box.height },
        { x: box.x + box.width, y: box.y + box.height }
    ];

    const effectiveRadius = Math.max(radius - circleInnerMarginPx, 0);

    return points.every(p => {
        const dx = p.x - circleX;
        const dy = p.y - circleY;
        const distance = Math.sqrt(dx * dx + dy * dy);
        return distance <= effectiveRadius;
    });
}

// Aqui hice cambios - Valida rostro listo para captura (simplificado)
function isFaceOptimal(detection, circleX, circleY, radius) {
    if (!detection || detection.score < minFaceConfidence) {
        return false;
    }

    const box = detection.box;
    const centerX = box.x + box.width / 2;
    const centerY = box.y + box.height / 2;
    const centerDx = centerX - circleX;
    const centerDy = centerY - circleY;
    const centerDistance = Math.sqrt(centerDx * centerDx + centerDy * centerDy);
    const effectiveRadius = radius - circleInnerMarginPx;

    if (centerDistance > effectiveRadius) {
        return false;
    }

    return true;
}


/**
 * Toma la instantánea y la dibuja en el elemento canvas
 */
function takeSnapshot() {
    snapshotCanvas.width = video.videoWidth;
    snapshotCanvas.height = video.videoHeight;
    ctxSnapshot.drawImage(video, 0, 0);
    stopVideo();
    video.classList.add('visually-hidden');
    circleOverlay.classList.add('visually-hidden');
    $("#faceid-helper").addClass('visually-hidden');
}


/**
 * Reinicia la configuración y transmite video de nuevo
 */
async function snapshotAgain() {
    activeVideoSectionLoader();
    captureBootstrapModal.hide();
    detectingFace = false;

    if (countdownActive) {
        clearInterval(countdownTimer);
        document.getElementById('countdown-overlay').style.display = 'none';
        countdownActive = false;
    }

    video.removeEventListener('playing', detectFace);

    await startVideo()
        .then(() => {
            readySnapshot = false;
            video.classList.remove('visually-hidden');
            circleOverlay.classList.remove('visually-hidden');
            $("#circle-overlay").removeClass("visually-hidden");
            detectingFace = true;
            video.addEventListener('playing', detectFace);
            $("#stop-video-btn-container").show();
        })
        .catch((err) => {
            console.error("❌ [snapshotAgain] Error al iniciar el video:", err);
        })
        .finally(() => {
            $("#loader-overlay").css('display', 'none');
        });
}


/**
 * Muestra el modal que contiene el canvas
 */
function openCaptureModal() {
    captureBootstrapModal.show();
}


/**
 * Inicia la cuenta regresiva para tomar la foto
 */
async function startCountdown() {
    countdownActive = true;
    circleOverlay.style.borderColor = "limegreen";
    let count = 3;
    const countdownEl = document.getElementById('countdown-overlay');
    countdownEl.style.display = 'block';
    countdownEl.textContent = count;

    await new Promise(resolve => setTimeout(resolve, 50)); // Esperar para que se actualice el estado

    countdownTimer = setInterval(() => {
        count--;
        if (count > 0) {
            countdownEl.textContent = count;
        } else {
            clearInterval(countdownTimer);
            countdownEl.style.display = 'none';
            countdownActive = false;
            countdownStartCenter = null;
            takeSnapshot();
            readySnapshot = true;
            openCaptureModal();
        }
    }, 1000);
}


/**
 * Detecta el rostro en la transmisión de la cámara
 */
async function detectFace() {
    if (!detectingFace) return;

    const detection = await faceapi
        .detectSingleFace(video, new faceapi.TinyFaceDetectorOptions());

    if (!detectingFace) return;

    const circleRect = circleOverlay.getBoundingClientRect();
    const videoRect = video.getBoundingClientRect();
    // Aqui hice cambios - Se agregó escala para convertir coordenadas del DOM a coordenadas del video
    const scaleX = video.videoWidth / videoRect.width;
    const scaleY = video.videoHeight / videoRect.height;
    const circleRadius = (circleRect.width / 2) * scaleX;
    const circleX = ((circleRect.left - videoRect.left) * scaleX) + circleRadius;
    const circleY = ((circleRect.top - videoRect.top) * scaleY) + circleRadius;

    // Aqui hice cambios - Verde solo durante cuenta regresiva
    const isValid = isFaceOptimal(detection, circleX, circleY, circleRadius);
    const now = Date.now();

    const faceCenter = detection
        ? { x: detection.box.x + detection.box.width / 2, y: detection.box.y + detection.box.height / 2 }
        : null;

    if (isValid) {
        if (stableSince === null) {
            stableSince = now;
        }
        invalidSince = null;
    } else {
        stableSince = null;
        if (invalidSince === null) {
            invalidSince = now;
        }
    }

    const isStable = stableSince !== null && (now - stableSince) >= stabilityMs;

    // Log para verificar el flujo
    if (countdownActive) {
        if (!isValid) {
            circleOverlay.style.borderColor = "red";
            clearInterval(countdownTimer);
            document.getElementById('countdown-overlay').style.display = 'none';
            countdownActive = false;
            countdownStartCenter = null;
            stableSince = null;
        } else {
            // Mantener verde mientras sea válido
            circleOverlay.style.borderColor = "limegreen";
        }
    } else {
        // No hay countdown activo - siempre rojo
        circleOverlay.style.borderColor = "red";
        if (isValid && isStable) {
            countdownStartCenter = faceCenter;
            await startCountdown();
            lastFaceCenter = faceCenter;
            // NO hacer return aqui - dejar que continue el loop de detección
        }
    }

    lastFaceCenter = faceCenter;

    // SIEMPRE continuar el loop de detección
    if (!readySnapshot && detectingFace) {
        requestAnimationFrame(detectFace);
    }
}


/**
 * Inicializa toda la funcionalidad de Face ID
 */
async function initFaceId() {
    video.removeEventListener('playing', detectFace);
    detectingFace = false;

    $("#faceid-config-btn").addClass('pointer-events disabled');
    activeVideoSectionLoader();
    $("#no-configured-alert").remove();

    await loadModels()
        .catch((err) => console.error("❌ [initFaceId] Error cargando modelos:", err));

    await startVideo()
        .then(() => {
            readySnapshot = false;
            detectingFace = true;
            video.classList.remove('visually-hidden');
            circleOverlay.classList.remove('visually-hidden');
            circleOverlay.style.borderColor = '';
            $("#circle-overlay").removeClass("visually-hidden");
            video.addEventListener('playing', detectFace);
            $("#stop-video-btn-container").show();
        })
        .finally(() => {
            $("#loader-overlay").css('display', 'none');
        });
}


// =============================
// 🎯 Eventos
// =============================
(() => {
    'use strict';

    $("#faceid-link").addClass("active");

    saveFaceIdButton.addEventListener('click', async function () {

        $("#save-faceid-button-container").html(`
            <button class="btn btn-primary" type="button" disabled>
                <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                Validando...
            </button>`);

        if (!canvasHaveContent(snapshotCanvas)) {
            $.notify({
                icon: 'icon-bell',
                title: 'Notificación',
                message: 'Debes capturar tu rostro'
            }, { type: 'warning' });
            return;
        }

        const faceDetection = await faceapi
            .detectSingleFace(snapshotCanvas, new faceapi.TinyFaceDetectorOptions())
            .withFaceLandmarks()
            .withFaceDescriptor();

        if (faceDetection == null || faceDetection == undefined) {
            $.notify({
                icon: 'icon-bell',
                title: 'Notificación',
                message: 'No se detectó ningún rostro en la captura'
            }, { type: 'warning' });
            return;
        }

        const descriptorArray = Array.from(faceDetection.descriptor);
        const formData = new FormData();

        snapshotCanvas.toBlob((blob) => {
            formData.append("image", blob, "capture.png");
            formData.append("descriptor", JSON.stringify(descriptorArray));

            $.ajax({
                url: '/empleado/faceid',
                method: 'POST',
                async: true,
                cache: false,
                contentType: false,
                processData: false,
                data: formData,
                beforeSend: () => {
                    $("#save-faceid-button-container").html(`
                        <button class="btn btn-primary" type="button" disabled>
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            Creando...
                        </button>`);
                },
                complete: () => {
                    $("#faceid-config-btn").remove();
                    captureBootstrapModal.hide();
                    setTimeout(() => {
                        $("#save-faceid-button-container")
                            .html(`<button id="save-faceid-button" type="button" class="btn btn-primary">Aprobar</button>`);
                    }, 1000);
                },
                success: (result) => {
                    if (result.success) {
                        drawFaceIDContent(result.data);
                    }
                    $.notify({
                        icon: 'icon-bell',
                        title: 'Notificación',
                        message: result.message
                    }, { type: result.success ? 'success' : 'warning' });
                },
                error: (err) => {
                    console.error("❌ [saveFaceId] Error en la petición:", err);
                    $.notify({
                        icon: 'icon-bell',
                        title: 'Notificación',
                        message: 'Ha ocurrido un error inesperado'
                    }, { type: 'error' });
                }
            });
        }, 'image/png');
    });

    video.addEventListener("loadeddata", () => {
        inactiveVideoSectionLoader();
    });

})();


/**
 * Dibuja la tarjeta que contiene la información del Face ID
 */
function drawFaceIDContent(data) {
    let faceidDiv = document.createElement('div');
    faceidDiv.classList.add('col-md-4');

    faceidDiv.innerHTML = `
    <div class="card card-post card-round">
        <div class="card-body">
            <div class="d-flex">
                <div class="avatar avatar-${data.active ? 'online' : 'offline'}">
                    <img src="${data.base64Image}" alt="..." class="avatar-img rounded-circle">
                </div>
                <div class="info-post ms-2">
                    <p class="username">Fecha registro</p>
                    <p class="date text-muted">${data.registerDate}</p>
                </div>
            </div>
            <div class="separator-solid"></div>
            <button class="btn btn-primary btn-rounded btn-sm" data-bs-toggle="collapse" data-bs-target="#faceid-image"
                    aria-expanded="true" aria-controls="faceid-image">
                Ver foto
            </button>
        </div>
        <div id="faceid-image" class="collapse">
            <img class="card-img-bottom" src="${data.base64Image}" alt="Card image cap">
        </div>
    </div>`;

    const faceidSectionDiv = document.getElementById("faceid-info-section");
    faceidSectionDiv.appendChild(faceidDiv);
}


/**
 * Valida si el elemento canvas contiene una imagen
 */
function canvasHaveContent(canvas) {
    const ctx = canvas.getContext('2d');
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;
    for (let i = 0; i < data.length; i += 4) {
        if (data[i] !== 0 || data[i + 1] !== 0 || data[i + 2] !== 0 || data[i + 3] !== 0) {
            return true;
        }
    }
    return false;
}
