import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

import environment from '../environments/environment';


@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.css',
    standalone: false
})
export default class AppComponent implements OnInit {
	public sizes = [72, 96, 128, 144, 152, 192, 384, 512];

	public isLoading = false;

	public size: number = 0;

	private image: File | null = null;

	public constructor(
		private readonly http: HttpClient
	) { }

	public ngOnInit() {
		const dropContainer = document.getElementById("dropcontainer")
		const fileInput = document.getElementById("images")

		dropContainer?.addEventListener("dragover", (e) => {
			e.preventDefault()
		}, false)

		dropContainer?.addEventListener("dragenter", () => {
			dropContainer?.classList.add("drag-active")
		})

		dropContainer?.addEventListener("dragleave", () => {
			dropContainer?.classList.remove("drag-active")
		})

		dropContainer?.addEventListener("drop", (e) => {
			e.preventDefault()
			dropContainer?.classList.remove("drag-active");

			(fileInput as any).files = e.dataTransfer!.files

			this.image = e.dataTransfer?.files[0] || null;
		})
	}

	public onFileChange(event: Event): void {
		const target = event.target as HTMLInputElement;

		this.image = target.files?.[0] || null;
	}

	public isAddSizeButtonDisabled(): boolean {
		return this.sizes.includes(this.size) || this.size <= 0;
	}

	public onSizeChange(event: Event): void {
		const target = event.target as HTMLInputElement;

		this.size = parseInt(target.value);
	}

	public addSize(): void {
		this.sizes.push(this.size);
		this.size = 0;
		this.sizes = this.sizes.sort((a, b) => a - b);
	}

	public removeSize(size: number): void {
		this.sizes = this.sizes.filter(s => s !== size);
	}

	public generateIcons(): void {
		this.isLoading = true;

		const form = new FormData();

		form.append("image", this.image as Blob);
		form.append("sizes", JSON.stringify(this.sizes));

		this.http.post(
			`${environment.apiUrl}images/create-icons`,
			form,
			{
				responseType: 'blob',
				headers: {
					Accept: "application/octet-stream"
				}
			}
		).subscribe((res) => {
			const url = URL.createObjectURL(res as any);
			const a = document.createElement("a");
			a.href = url;
			a.download = "icons.zip";
			a.click();
			URL.revokeObjectURL(url);

			this.isLoading = false;
		});
	}

	public isGenerateButtonDisabled(): boolean {
		return this.sizes.length === 0 || !this.image;
	}
}
