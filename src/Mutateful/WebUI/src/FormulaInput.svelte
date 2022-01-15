<script lang="ts">
    import {onMount} from "svelte";

    let offset = 0;
    let charWidthInPixels = 8;

    onMount(() => {
        charWidthInPixels = (document.querySelector(".char-width-reference") as HTMLElement).offsetWidth;
    });
    
    function handleKeyup(event: Event) {
        offset = charWidthInPixels * (event.target as HTMLInputElement).selectionStart;
        if (offset > 0) offset -= 4;
    }
</script>

<code class="char-width-reference">M</code>
<input class="formula-editor" type="text" on:keyup={handleKeyup}>
<div class="autocomplete-list" style="left: {offset}px;"></div>

<style>
    .char-width-reference {
        padding: 0;
        margin: 0;
        width: auto;
        border-width: 0;
        position: absolute;
        left: -100px;
        top: -100px;
    }
    input {
        box-sizing: border-box;
        width: 100%;
        font-family: Consolas, "Lucida Console", monospace;
    }
    .autocomplete-list {
        position: absolute;
        width: 150px;
        height: 4rem;
        background-color: #2f4f4f;
        color: #ffffff;
    }
</style>