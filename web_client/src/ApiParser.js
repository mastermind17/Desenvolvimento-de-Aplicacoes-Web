/*
{
	comments_url: "/api/projects/{projectName}/issues/{issueId}/comments{/commentId}",
	issues_url: "/api/projects/{projectName}/issues{/issueId}",
	projects_url: "/api/projects{/projectName}",
	tags_url: "/api/projects/{projectName}/tags{/tagName}"
}
 */

function ApiParser(obj){
	this.resourcesUris = obj;
	this.relations = {
		projectName : "projectName",
		issuesID: "issueId",
		commentID: "commentId",
		tagName: "tagName",
		containsName: "containsName"
	}
}

ApiParser.prototype.getAllProjectsUrl= function (){
	return this.resourcesUris.projects_url.split(`{/${this.relations.projectName}}`)[0];
}

ApiParser.prototype.queryAllProjectsUri= function(query){
	return `${this.getAllProjectsUri()}?${this.relations.containsName}=${query}`;
}

ApiParser.prototype.queryAllIssuesUri= function(query){
	return `${this.getAllIssuesUri()}?${this.relations.containsName}=${query}`;
}


ApiParser.prototype.getAllIssuesUri= function(projName){
	return this.resourcesUris.issues_url
				.replace(`{${this.relations.projectName}}`, `${projName}`)
				.split(`{/${this.relations.issuesID}}`)[0];
}

/*

ApiParser.prototype.getProjectByNameUri= function(name){
	return this.resourcesUris.projects_url
				.replace(`{/${this.relations.projectName}}`, `${name}`);
}

ApiParser.prototype.getIssueByIdUri= function(projName, issueID){
	return this.resourcesUris.issues_url
				.replace(`{/${this.relations.projectName}}`, `/${projName}`)
				.replace(`{/${this.relations.issuesID}}`, `${issueID}`);
}

ApiParser.prototype.getTagsUri= function(projName){
	return this.resourcesUris.tags_url
				.replace(`{/${this.relations.projectName}}`, `/${projName}`)
				.split(`{/${this.relations.tagName}}`)[0];
}

ApiParser.prototype.getTagByNameUri= function(projName, tagName){
	return this.resourcesUris.tags_url
				.replace(`{/${this.relations.projectName}}`, `/${projName}`)
				.replace(`{/${this.relations.tagName}}`, `/${tagName}`)
}

ApiParser.prototype.getCommentsUri= function(projName, issueId){
	return this.resourcesUris.comments_url
				.replace(`{/${this.relations.projectName}}`, `/${projName}`)
				.replace(`{/${this.relations.issuesID}}`, `/${issueId}`)
				.split(`{/${this.relations.commentID}}`)[0];
}

ApiParser.prototype.getCommentByIdUri= function(projName, issueId, commentId){
	return this.resourcesUris.comments_url
				.replace(`{/${this.relations.projectName}}`, `/${projName}`)
				.replace(`{/${this.relations.issuesID}}`, `/${issueId}`)
				.replace(`{/${this.relations.commentID}}`, `/${commentId}`)
}
*/

export default ApiParser;