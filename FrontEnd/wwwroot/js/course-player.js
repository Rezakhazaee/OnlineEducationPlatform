window.coursePlayer = {
    get(id) { return document.getElementById(id); },
    toggle(id) {
        const v = this.get(id);
        if (!v) return;
        v.paused ? v.play() : v.pause();
    },
    seek(id, seconds) {
        const v = this.get(id);
        if (v && Number.isFinite(v.duration))
            v.currentTime = Math.max(0, Math.min(v.duration, v.currentTime + seconds));
    },
    setSpeed(id, speed) {
        const v = this.get(id);
        if (v) v.playbackRate = Number(speed);
    },
    setVolume(id, volume) {
        const v = this.get(id);
        if (v) {
            v.volume = Math.max(0, Math.min(1, Number(volume)));
            v.muted = v.volume === 0;
        }
    },
    toggleMute(id) {
        const v = this.get(id);
        if (v) v.muted = !v.muted;
    },
    async fullscreen(id) {
        const v = this.get(id);
        if (!v) return;
        const box = v.closest('.course-player-shell') || v;
        if (document.fullscreenElement)
            await document.exitFullscreen();
        else if (box.requestFullscreen)
            await box.requestFullscreen();
    },
    async pip(id) {
        const v = this.get(id);
        if (!v || !document.pictureInPictureEnabled) return;
        if (document.pictureInPictureElement)
            await document.exitPictureInPicture();
        else if (v.readyState >= 1)
            await v.requestPictureInPicture();
    }
};
